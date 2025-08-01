using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Race/Signals/Race Started Signal", fileName = "RaceStartedSignal")]
public sealed class RaceStartedSignal : WebGLSignalBase
{
    private event Action _onRaised;
    private readonly HashSet<string> _listenerNames = new HashSet<string>();

    public void AddListener(Action callback, string listenerName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log("Attempted to add null callback to RaceStartedSignal", LogType.Warning, "Signals");
            return;
        }

        // Проверяем, не добавлен ли уже этот callback
        if (IsCallbackAlreadyAdded(callback))
        {
            WebGLDebugLogger.Log($"Callback from {listenerName} already added to RaceStartedSignal", LogType.Warning, "Signals");
            return;
        }

        _onRaised += callback;
        _listenerNames.Add(listenerName);
        _listenersCount = GetInvocationListCount();

        LogListenerChange("added", listenerName);
    }

    public void AddListener(Action callback) => AddListener(callback, "Legacy");

    public void RemoveListener(Action callback, string listenerName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log("Attempted to remove null callback from RaceStartedSignal", LogType.Warning, "Signals");
            return;
        }

        _onRaised -= callback;
        _listenerNames.Remove(listenerName);
        _listenersCount = GetInvocationListCount();

        LogListenerChange("removed", listenerName);
    }

    public void RemoveListener(Action callback) => RemoveListener(callback, "Legacy");

    public void Raise()
    {
        LogSignalRaised();

        try
        {
            _onRaised?.Invoke();
            WebGLDebugLogger.Log($"RaceStartedSignal raised successfully to {_listenersCount} listeners", LogType.Log, "Signals");
        }
        catch (Exception ex)
        {
            WebGLDebugLogger.Log($"Error raising RaceStartedSignal: {ex.Message}", LogType.Error, "Signals");
            WebGLDebugLogger.Log($"Stack trace: {ex.StackTrace}", LogType.Error, "Signals");
        }
    }

    private int GetInvocationListCount()
    {
        return _onRaised?.GetInvocationList()?.Length ?? 0;
    }

    private bool IsCallbackAlreadyAdded(Action callback)
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

        WebGLDebugLogger.Log($"Removed all {removedCount} listeners from RaceStartedSignal", LogType.Log, "Signals");
        LogListenerChange("all removed");
    }

    [ContextMenu("Test Raise")]
    private void TestRaise()
    {
        Raise();
    }
}
