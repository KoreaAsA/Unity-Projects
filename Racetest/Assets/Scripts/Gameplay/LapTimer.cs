using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public sealed class LapTimer : MonoBehaviour
{
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;

    private float _startTime;
    private bool _running;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[LapTimer] Нет Collider!");
            return;
        }
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        _raceStartedSignal.AddListener(OnRaceStarted);
    }

    private void OnDisable()
    {
        _raceStartedSignal.RemoveListener(OnRaceStarted);
    }

    private void OnRaceStarted()
    {
        _startTime = Time.time;
        _running = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_running) return;

        _running = false;
        _raceFinishedSignal.Raise(Time.time - _startTime);
    }
}