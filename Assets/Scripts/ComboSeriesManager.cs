using System.Collections.Generic;
using UnityEngine;

public class ComboSeriesManager : MonoBehaviour
{
    public static ComboSeriesManager Instance;
    public int defaultComboBonus = 5;

    class SeriesState { public int total; public int sliced; public bool failed; public int bonus; }
    private readonly Dictionary<int, SeriesState> _series = new();

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    public void StartSeries(int id, int total, int bonus)
    {
        _series[id] = new SeriesState { total = total, sliced = 0, failed = false, bonus = bonus };
    }

    public void RegisterSlice(int id)
    {
        if (!_series.TryGetValue(id, out var s) || s.failed) return;
        s.sliced++;
        if (s.sliced >= s.total && !s.failed)
        {
            ScoreManager.instance?.AddScore(s.bonus);
            _series.Remove(id);
        }
    }

    public void RegisterMiss(int id)
    {
        if (_series.TryGetValue(id, out var s)) s.failed = true;
    }
}

