using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Race/GhostTrackStorage")]
public sealed class GhostTrackStorage : ScriptableObject
{
    private readonly List<VehicleSnapshot> _frames = new();
    public IReadOnlyList<VehicleSnapshot> Frames => _frames;

    public void Clear() => _frames.Clear();
    public void Add(VehicleSnapshot frame) => _frames.Add(frame);
}