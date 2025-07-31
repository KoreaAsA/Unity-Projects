using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class LapTimer : MonoBehaviour
{
    public event Action<float> OnLapCompleted;

    private float _startTime;
    private bool _running;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    public void StartTimer()
    {
        _startTime = Time.time;
        _running = true;
    }

    public void StopTimer() => _running = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_running) return;
        _running = false;
        OnLapCompleted?.Invoke(Time.time - _startTime);
    }
}