using UnityEngine;

public class Blade : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask sliceableLayer;

    [Header("Thresholds (world units)")]
    public float activateAfterDistance = 0.12f;   // movement needed to START slicing
    public float minSegmentDistance   = 0.02f;    // movement between slices

    [Header("UI")]
    public GameObject pausePanel; 

    // 🔒 Global lock set when a bomb is sliced
    public static bool IsLocked { get; private set; }

    private Vector2 startPosition;     // where the touch/click began
    private Vector2 previousPosition;  // last position we sliced from
    private TrailRenderer trailRenderer;

    private bool isTracking = false;   // finger down / mouse down
    private bool isSlicing  = false;   // trail + slicing active

    void Awake()
    {
        IsLocked = false; // auto-unlock on scene load/enable
    }

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer) trailRenderer.enabled = false;
    }

    void Update()
    {
        // Don’t handle input if paused or locked
        if ((pausePanel && pausePanel.activeSelf) || IsLocked)
        {
            if (trailRenderer && trailRenderer.enabled) trailRenderer.enabled = false;
            return;
        }

    #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
    #else
        HandleTouch();
    #endif
    }

    // ---------------- Public Lock API ----------------
    public static void LockSlicing()
    {
        IsLocked = true;
        // Hide any active trail immediately
        var blade = FindObjectOfType<Blade>();
        if (blade && blade.trailRenderer) blade.trailRenderer.enabled = false;
    }

    public static void UnlockSlicing()
    {
        IsLocked = false;
    }

    // ---------------- Mouse ----------------
    void HandleMouse()
    {
        if (IsLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            isTracking = true;
            isSlicing = false;

            startPosition    = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            previousPosition = startPosition;

            if (trailRenderer) trailRenderer.enabled = false; // don't show trail yet
        }

        if (Input.GetMouseButton(0) && isTracking)
        {
            Vector2 current = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // If not slicing yet, wait until movement passes activation distance
            if (!isSlicing)
            {
                if ((current - startPosition).sqrMagnitude >= activateAfterDistance * activateAfterDistance)
                {
                    isSlicing = true;
                    if (trailRenderer) trailRenderer.enabled = true;
                }
            }

            // Only slice when active and moved enough since last segment
            if (isSlicing && (current - previousPosition).sqrMagnitude >= minSegmentDistance * minSegmentDistance)
            {
                TrySlice(previousPosition, current);
                previousPosition = current;
                transform.position = current;
            }
        }

        if (Input.GetMouseButtonUp(0)) StopSlicing();
    }

    // ---------------- Touch ----------------
    void HandleTouch()
    {
        if (IsLocked || Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        Vector2 current = mainCamera.ScreenToWorldPoint(touch.position);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isTracking = true;
                isSlicing  = false;

                startPosition    = current;
                previousPosition = current;

                if (trailRenderer) trailRenderer.enabled = false; // no trail on long press
                break;

            case TouchPhase.Moved:
                if (!isTracking) break;

                if (!isSlicing)
                {
                    // require real movement to activate
                    if ((current - startPosition).sqrMagnitude >= activateAfterDistance * activateAfterDistance)
                    {
                        isSlicing = true;
                        if (trailRenderer) trailRenderer.enabled = true;
                    }
                }

                if (isSlicing && (current - previousPosition).sqrMagnitude >= minSegmentDistance * minSegmentDistance)
                {
                    TrySlice(previousPosition, current);
                    previousPosition = current;
                    transform.position = current;
                }
                break;

            case TouchPhase.Stationary:
                // ignore; this prevents long-press from creating trails/slices
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                StopSlicing();
                break;
        }
    }

    void StopSlicing()
    {
        isTracking = false;
        isSlicing  = false;
        if (trailRenderer) trailRenderer.enabled = false;
    }

    void TrySlice(Vector2 start, Vector2 end)
    {
        if (IsLocked) return; // extra guard

        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, sliceableLayer);
        Debug.DrawLine(start, end, Color.red, 0.1f);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Fruit"))
            {
                Fruit fruit = hit.collider.GetComponent<Fruit>();
                if (fruit != null) fruit.Slice();
            }
        }
    }
}
