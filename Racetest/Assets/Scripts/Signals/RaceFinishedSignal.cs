using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Race/Signals/Race Finished Signal", fileName = "RaceFinishedSignal")]
public sealed class RaceFinishedSignal : WebGLSignalBase
{
    private event Action<float> _onRaised;
    private readonly HashSet<string> _listenerNames = new HashSet<string>();

    public void AddListener(Action<float> callback, string listenerName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log("Attempted to add null callback to RaceFinishedSignal", LogType.Warning, "Signals");
            return;
        }

        if (IsCallbackAlreadyAdded(callback))
        {
            WebGLDebugLogger.Log($"Callback from {listenerName} already added to RaceFinishedSignal", LogType.Warning, "Signals");
            return;
        }

        _onRaised += callback;
        _listenerNames.Add(listenerName);
        _listenersCount = GetInvocationListCount();

        LogListenerChange("added", listenerName);
    }

    public void AddListener(Action<float> callback) => AddListener(callback, "Legacy");

    public void RemoveListener(Action<float> callback, string listenerName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log("Attempted to remove null callback from RaceFinishedSignal", LogType.Warning, "Signals");
            return;
        }

        _onRaised -= callback;
        _listenerNames.Remove(listenerName);
        _listenersCount = GetInvocationListCount();

        LogListenerChange("removed", listenerName);
    }

    public void RemoveListener(Action<float> callback) => RemoveListener(callback, "Legacy");

    public void Raise(float lapTime)
    {
        LogSignalRaised($"lapTime: {lapTime:F2}s");

        try
        {
            _onRaised?.Invoke(lapTime);
            WebGLDebugLogger.Log($"RaceFinishedSignal raised successfully with lapTime {lapTime:F2}s to {_listenersCount} listeners", LogType.Log, "Signals");
        }
        catch (Exception ex)
        {
            WebGLDebugLogger.Log($"Error raising RaceFinishedSignal: {ex.Message}", LogType.Error, "Signals");
            WebGLDebugLogger.Log($"Stack trace: {ex.StackTrace}", LogType.Error, "Signals");
        }
    }

    private int GetInvocationListCount()
    {
        return _onRaised?.GetInvocationList()?.Length ?? 0;
    }

    private bool IsCallbackAlreadyAdded(Action<float> callback)
    {
        if (_onRaised == null) return false;

        var invocationList = _onRaised.GetInvocationList();
        foreach (var invocation in invocationList)
        {
            if (invocation.Equals(callback))
                return true;
        }
        return false;
    }

    public void RemoveAllListeners()
    {
        int removedCount = _listenersCount;
        _onRaised = null;
        _listenerNames.Clear();
        _listenersCount = 0;

        WebGLDebugLogger.Log($"Removed all {removedCount} listeners from RaceFinishedSignal", LogType.Log, "Signals");
        LogListenerChange("all removed");
    }

    [ContextMenu("Test Raise")]
    private void TestRaise()
    {
        Raise(123.45f);
    }
}