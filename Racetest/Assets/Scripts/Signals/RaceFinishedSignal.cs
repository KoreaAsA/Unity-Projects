using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Race/Signals/Race Finished Signal")]
public sealed class RaceFinishedSignal : ScriptableObject
{
    private event Action<float> _onRaised;
    public void AddListener(Action<float> a) => _onRaised += a;
    public void RemoveListener(Action<float> a) => _onRaised -= a;
    public void Raise(float lapTime) => _onRaised?.Invoke(lapTime);
}