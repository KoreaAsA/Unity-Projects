using UnityEngine;
using System;
using System.Collections.Generic;
public class WebGLSignalBase : ScriptableObject
{
    [Header("Debug Info")]
    [SerializeField] protected bool _verboseLogging = true;

    protected int _listenersCount = 0;
    protected int _raisedCount = 0;
    protected float _lastRaisedTime = 0f;

    protected virtual void OnEnable()
    {
        _raisedCount = 1;
        WebGLDebugLogger.Log($"{GetType().Name} enabled", LogType.Log, "Signals");
        WebGLDebugLogger.SetSystemState($"Signal_{GetType().Name}", true);
    }

    protected virtual void OnDisable()
    {
        WebGLDebugLogger.Log($"{GetType().Name} disabled", LogType.Log, "Signals");
        WebGLDebugLogger.SetSystemState($"Signal_{GetType().Name}", false);
    }

    protected void LogListenerChange(string action, string listenerInfo = "")
    {
        if (_verboseLogging)
        {
            string message = string.IsNullOrEmpty(listenerInfo) ?
                $"Listener {action}" :
                $"Listener {action}: {listenerInfo}";
            WebGLDebugLogger.Log($"[{GetType().Name}] {message}", LogType.Log, "Signals");
        }

        WebGLDebugLogger.SetSystemValue($"{GetType().Name}_Listeners", _listenersCount.ToString());
    }

    protected void LogSignalRaised(string data = "")
    {
        _raisedCount++;
        _lastRaisedTime = Time.time;

        WebGLDebugLogger.LogSignalFired(GetType().Name, data);
        WebGLDebugLogger.SetSystemValue($"{GetType().Name}_RaisedCount", _raisedCount.ToString());
        WebGLDebugLogger.SetSystemValue($"{GetType().Name}_LastRaised", _lastRaisedTime.ToString("F2"));
    }

    // Методы для отладки в инспекторе
    [ContextMenu("Show Debug Info")]
    private void ShowDebugInfo()
    {
        Debug.Log($"{GetType().Name} Debug Info:\n" +
                 $"Listeners: {_listenersCount}\n" +
                 $"Raised Count: {_raisedCount}\n" +
                 $"Last Raised: {_lastRaisedTime:F2}s");
    }

    [ContextMenu("Reset Debug Info")]
    private void ResetDebugInfo()
    {
        _raisedCount = 0;
        _lastRaisedTime = 0f;
        WebGLDebugLogger.Log($"{GetType().Name} debug info reset", LogType.Log, "Signals");
    }
}