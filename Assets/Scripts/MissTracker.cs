using UnityEngine;
using System;

[DefaultExecutionOrder(-1000)] // init before almost everything
public class MissTracker : MonoBehaviour
{
    private static MissTracker _instance;
    public static MissTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MissTracker>();
                if (_instance == null)
                {
                    // Auto-create if missing (mobile-safe)
                    var go = new GameObject("MissTracker(Auto)");
                    _instance = go.AddComponent<MissTracker>();
                    DontDestroyOnLoad(go);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("[MissTracker] Auto-created instance.");
#endif
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Miss Settings")]
    public int maxMisses = 5;

    [Header("Runtime (read-only)")]
    public int missCount = 0;
    public bool isGameOver = false;

    public event Action<int, int> OnMissChanged;
    public event Action OnGameOver;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetMisses()
    {
        isGameOver = false;
        missCount = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[MissTracker] ResetMisses()");
#endif
        OnMissChanged?.Invoke(missCount, maxMisses);
    }

    public void RegisterMiss()
    {
        if (isGameOver) return;
        missCount++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[MissTracker] RegisterMiss -> {missCount}/{maxMisses}");
#endif
        OnMissChanged?.Invoke(missCount, maxMisses);

        if (missCount >= maxMisses)
        {
            isGameOver = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[MissTracker] Reached maxMisses -> GAME OVER event");
#endif
            OnGameOver?.Invoke();
        }
    }
}
