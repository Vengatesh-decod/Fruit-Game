using UnityEngine;

public class Fruit : MonoBehaviour
{
    [Header("Fruit Settings")]
    public bool isBomb = false;
    public GameObject slicedFruitPrefab;
    public ParticleSystem splashEffect;

    [Header("Audio")]
    public AudioClip sliceSound;
    public AudioClip bombSound;
    public string sfxName = "slice";

    private bool isSliced = false;
    [HideInInspector] public int seriesId = -1;

    // auto-destroy Y limit
    private float destroyY;

    void Start()
    {
        // Get spawner Y position and go a bit lower
        FruitSpawner spawner = FindObjectOfType<FruitSpawner>();
        if (spawner != null) destroyY = spawner.yPosition - 1f;
        else destroyY = -7f; // fallback
    }

    void Update()
    {
        // Auto destroy when fruit falls below destroyY
        if (!isSliced && transform.position.y < destroyY)
        {
            // Count as a MISS only for normal fruits (ignore bombs)
            if (!isBomb)
                MissTracker.Instance?.RegisterMiss();

            if (seriesId >= 0)
                ComboSeriesManager.Instance?.RegisterMiss(seriesId);

            Destroy(gameObject);
        }
    }

    public void Slice()
    {
        if (isSliced) return;
        isSliced = true;

        // Slice SFX (generic)
        if (sliceSound != null)
            AudioManager.Instance?.PlaySFX("slice", false);

        // ----- Bomb path: trigger immediate Game Over after a short delay -----
        if (isBomb)
        {
            Blade.LockSlicing(); // stop further slicing immediately

            AudioManager.Instance?.PlaySFX("bomb", false);

            if (splashEffect != null)
                Instantiate(splashEffect, transform.position, Quaternion.identity);

            var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
            var sr  = GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false;

            StartCoroutine(DelayAndGameOver());
            return;
        }

        // ----- Normal fruit slice path -----
        if (slicedFruitPrefab != null)
        {
            GameObject sliced = Instantiate(slicedFruitPrefab, transform.position, transform.rotation);
            Rigidbody2D[] slices = sliced.GetComponentsInChildren<Rigidbody2D>();
            foreach (var rb in slices)
                rb.AddForce(Random.insideUnitCircle * 5f, ForceMode2D.Impulse);

            Destroy(sliced, 4f);
        }

        if (splashEffect != null)
            Instantiate(splashEffect, transform.position, Quaternion.identity);

        if (seriesId >= 0)
        {
            ScoreManager.instance?.AddScore(1, true);
            ComboSeriesManager.Instance?.RegisterSlice(seriesId);
        }
        else
        {
            ScoreManager.instance?.AddScore(1);
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator DelayAndGameOver()
    {
        // Sync to bomb clip if available
        float delay = 1f;
        if (bombSound != null && bombSound.length > 0f)
            delay = Mathf.Clamp(bombSound.length * 0.9f, 0.2f, 2.5f);

        yield return new WaitForSecondsRealtime(delay);
        GameManager.instance?.GameOver();
        Destroy(gameObject);
    }

    public bool IsSliced => isSliced;
}


