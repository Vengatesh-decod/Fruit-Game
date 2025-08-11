using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FruitSpawner : MonoBehaviour
{
    [Header("Camera / Dynamic Bounds")]
    public Camera cam;
    public float screenMarginX = 0.25f;
    public float spawnBelowScreen = 0.4f;

    [Header("Prefabs")]
    public GameObject[] fruitPrefabs;
    public GameObject bombPrefab;

    [Header("Spawn Area (computed)")]
    [SerializeField] public float xRange = 2.5f;
    [SerializeField] public float yPosition = -6f;

    [Header("Timing")]
    public float minInterval = 0.6f;
    public float maxInterval = 1.2f;

    [Header("Bomb Settings")]
    [Range(0f, 1f)] public float bombChance = 0.12f;

    [Header("Launch Physics")]
    public Vector2 sideForceX = new Vector2(-1.2f, 1.2f);
    public Vector2 upForceY = new Vector2(11f, 16f);
    public Vector2 torqueZ = new Vector2(-220f, 220f);

    [Header("Misc")]
    public bool randomizeRotationOnSpawn = true;

    [Header("Series (Burst)")]
    [Range(0f, 1f)] public float seriesChance = 0.25f;
    public Vector2Int seriesCountRange = new Vector2Int(3, 5);
    public int seriesBonus = 5;
    public bool allowBombInSeries = false;

    public enum SeriesPattern { RandomX, Line, Fan }
    public SeriesPattern pattern = SeriesPattern.RandomX;
    public float lineSpacing = 1.2f;
    public float fanSpread = 60f;

    private int _nextSeriesId = 1;
    private int _lastScreenW, _lastScreenH;
    private bool _isStopped = false;

    // NEW: Track all spawned objects
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        RecalculateBounds();
        _lastScreenW = Screen.width;
        _lastScreenH = Screen.height;
    }

    private void OnEnable()
    {
        RecalculateBounds();
    }

    private void Update()
    {
        if (_lastScreenW != Screen.width || _lastScreenH != Screen.height)
        {
            _lastScreenW = Screen.width;
            _lastScreenH = Screen.height;
            RecalculateBounds();
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private void RecalculateBounds()
    {
        if (cam == null) return;

        if (cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            xRange = Mathf.Max(0.05f, halfW - screenMarginX);
            yPosition = cam.transform.position.y - halfH - spawnBelowScreen;
        }
        else
        {
            float zDist = Mathf.Abs((transform.position - cam.transform.position).z);
            Vector3 worldLeftBottom = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDist));
            Vector3 worldRightBottom = cam.ViewportToWorldPoint(new Vector3(1f, 0f, zDist));
            Vector3 worldCenterBottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, zDist));

            float halfWidth = (worldRightBottom.x - worldLeftBottom.x) * 0.5f;
            xRange = Mathf.Max(0.05f, halfWidth - screenMarginX);
            yPosition = worldCenterBottom.y - spawnBelowScreen;
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (!_isStopped)
        {
            if (Random.value < seriesChance)
                SpawnSeries();
            else
                SpawnOne();

            float wait = Random.Range(minInterval, maxInterval);
            float elapsed = 0f;
            while (elapsed < wait && !_isStopped)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }
    }

    private void SpawnOne()
    {
        if (_isStopped) return;

        GameObject prefab = PickSinglePrefab();
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-xRange, xRange), yPosition, 0f);
        Quaternion rot = randomizeRotationOnSpawn ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;

        GameObject obj = Instantiate(prefab, spawnPos, rot);
        spawnedObjects.Add(obj); // Track it

        var fruit = obj.GetComponent<Fruit>();
        if (fruit != null) fruit.seriesId = -1;

        Kick(obj);
    }

    private void SpawnSeries()
    {
        if (_isStopped) return;

        int count = Random.Range(seriesCountRange.x, seriesCountRange.y + 1);
        int id = _nextSeriesId++;
        ComboSeriesManager.Instance?.StartSeries(id, count, seriesBonus);

        for (int i = 0; i < count && !_isStopped; i++)
        {
            GameObject prefab = PickSeriesPrefab();
            if (prefab == null) continue;

            Vector3 spawnPos = GetSeriesPosition(i, count);
            Quaternion rot = randomizeRotationOnSpawn ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;

            GameObject obj = Instantiate(prefab, spawnPos, rot);
            spawnedObjects.Add(obj); // Track it

            var fruit = obj.GetComponent<Fruit>();
            if (fruit != null) fruit.seriesId = id;

            Kick(obj, i, count);
        }
    }

    private GameObject PickSinglePrefab()
    {
        if (bombPrefab != null && Random.value < bombChance) return bombPrefab;
        if (fruitPrefabs == null || fruitPrefabs.Length == 0)
        {
            Debug.LogWarning("FruitSpawner: No fruitPrefabs assigned.");
            return null;
        }
        return fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
    }

    private GameObject PickSeriesPrefab()
    {
        if (!allowBombInSeries && fruitPrefabs != null && fruitPrefabs.Length > 0)
            return fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
        return PickSinglePrefab();
    }

    private Vector3 GetSeriesPosition(int index, int total)
    {
        switch (pattern)
        {
            case SeriesPattern.Line:
                float totalWidth = lineSpacing * (total - 1);
                float startX = -totalWidth * 0.5f;
                float xL = Mathf.Clamp(startX + index * lineSpacing, -xRange, xRange);
                return new Vector3(xL, yPosition, 0f);

            case SeriesPattern.Fan:
                float t = (total > 1) ? (index / (float)(total - 1)) : 0.5f;
                float xF = Mathf.Lerp(-xRange * 0.8f, xRange * 0.8f, t);
                return new Vector3(xF, yPosition, 0f);

            case SeriesPattern.RandomX:
            default:
                float xR = Random.Range(-xRange, xRange);
                return new Vector3(xR, yPosition, 0f);
        }
    }

    private void Kick(GameObject obj, int index = 0, int total = 1)
    {
        if (obj == null) return;
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 impulse = new Vector2(Random.Range(sideForceX.x, sideForceX.y),
                                      Random.Range(upForceY.x, upForceY.y));

        if (pattern == SeriesPattern.Fan && total > 1)
        {
            float t = (index / (float)(total - 1)) - 0.5f;
            float angle = t * fanSpread;
            float rad = angle * Mathf.Deg2Rad;
            impulse.x += Mathf.Sin(rad) * 1.2f;
            impulse.y += Mathf.Cos(rad) * 0.6f;
        }

        rb.AddForce(impulse, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(torqueZ.x, torqueZ.y), ForceMode2D.Impulse);
    }

    public void StopSpawning()
    {
        if (_isStopped) return;
        _isStopped = true;

        StopAllCoroutines();

        // Destroy all tracked spawned objects
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }
}
