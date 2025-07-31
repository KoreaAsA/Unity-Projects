using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class Recorder : MonoBehaviour, IRecorder
{
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;

    private readonly List<VehicleSnapshot> _frames = new();
    private Rigidbody _rb;
    private bool _isRecording;
    private float _recordingStartTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("[Recorder] Rigidbody component not found!");
        }
    }

    public void RecordFrame(VehicleSnapshot snapshot) => _frames.Add(snapshot);
    public IReadOnlyList<VehicleSnapshot> GetTrajectory() => _frames;
    public void Clear()
    {
        _frames.Clear();
        Debug.Log($"[Recorder] Trajectory cleared");
    }

    private void OnEnable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.AddListener(StartRecording);
        }

        if (_raceFinishedSignal != null)
        {
            _raceFinishedSignal.AddListener(StopRecording);
        }

        Debug.Log("[Recorder] Event listeners added");
    }

    private void OnDisable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.RemoveListener(StartRecording);
        }

        if (_raceFinishedSignal != null)
        {
            _raceFinishedSignal.RemoveListener(StopRecording);
        }

        Debug.Log("[Recorder] Event listeners removed");
    }

    private void StartRecording()
    {
        _isRecording = true;
        _recordingStartTime = Time.time;
        Debug.Log("[Recorder] Recording started");
    }

    private void StopRecording(float lapTime)
    {
        _isRecording = false;
        Debug.Log($"[Recorder] Recording stopped. Recorded {_frames.Count} frames over {lapTime:F2} seconds");
    }

    private void FixedUpdate()
    {
        if (!_isRecording || _rb == null) return;

        float relativeTime = Time.time - _recordingStartTime;

        var snapshot = new VehicleSnapshot(
            relativeTime, //Используем относительное время от старта записи
            _rb.position,
            _rb.rotation,
            _rb.linearVelocity,
            _rb.angularVelocity
        );

        RecordFrame(snapshot);
    }
}