using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Race/Ghost Track Storage")]
public class GhostTrackStorage : ScriptableObject
{
    [SerializeField] private List<VehicleSnapshot> _frames = new List<VehicleSnapshot>();

    // Флаг что траектория уже записана
    [SerializeField] private bool _isFirstTrajectoryRecorded = false;

    public IReadOnlyList<VehicleSnapshot> Frames => _frames;

    // Проверка записана ли траектория
    public bool IsTrajectoryRecorded => _isFirstTrajectoryRecorded && _frames.Count > 0;

    public void Add(VehicleSnapshot snapshot)
    {
        _frames.Add(snapshot);
    }

    public void Clear()
    {
        _frames.Clear();
    }

    // Сохранение первой траектории
    public bool TrySaveFirstTrajectory(IReadOnlyList<VehicleSnapshot> trajectory)
    {
        if (_isFirstTrajectoryRecorded)
        {
            Debug.Log("[GhostTrackStorage] First trajectory already recorded, skipping save");
            return false;
        }

        _frames.Clear();
        foreach (var frame in trajectory)
        {
            _frames.Add(frame);
        }

        _isFirstTrajectoryRecorded = true;
        Debug.Log($"[GhostTrackStorage] First trajectory saved with {_frames.Count} frames");
        return true; // Успешно сохранили первую траекторию
    }

    // Полный сброс (для разработки/тестов)
    [ContextMenu("Reset First Trajectory (Dev Only)")]
    public void ResetFirstTrajectory()
    {
        _frames.Clear();
        _isFirstTrajectoryRecorded = false;
        Debug.Log("[GhostTrackStorage] First trajectory reset (dev mode)");
    }
}