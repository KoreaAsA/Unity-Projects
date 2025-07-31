using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class Recorder : MonoBehaviour, IRecorder
{
    private readonly List<VehicleSnapshot> _frames = new();
    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void RecordFrame(VehicleSnapshot snapshot)
    {
        _frames.Add(snapshot);
    }

    public IReadOnlyList<VehicleSnapshot> GetTrajectory() => _frames;

    public void Clear() => _frames.Clear();

    private void FixedUpdate()
    {
        RecordFrame(new VehicleSnapshot(
            Time.time,
            _rb.position,
            _rb.rotation,
            _rb.velocity,
            _rb.angularVelocity));
    }
}