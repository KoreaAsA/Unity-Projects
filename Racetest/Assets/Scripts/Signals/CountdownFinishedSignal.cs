using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Race/Signals/Countdown Finished Signal")]
public sealed class CountdownFinishedSignal : ScriptableObject
{
    private event Action _onRaised;
    public void AddListener(Action a) => _onRaised += a;
    public void RemoveListener(Action a) => _onRaised -= a;
    public void Raise() => _onRaised?.Invoke();
}