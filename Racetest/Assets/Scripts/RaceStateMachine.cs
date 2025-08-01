using UnityEngine;
using System.Collections;

/// <summary>
/// WebGL-адаптированная версия RaceStateMachine с подробным логированием
/// </summary>
public sealed class RaceStateMachine : MonoBehaviour
{
    public static RaceStateMachine Instance { get; private set; }

    public System.Action<RaceState> OnStateChanged;
    public RaceState Current { get; private set; } = RaceState.Idle;

    [Header("WebGL Debug")]
    [SerializeField] private bool _verboseLogging = true;

    // Для отслеживания подписчиков
    private int _subscribersCount = 0;

    private void Awake()
    {
        WebGLDebugLogger.Log("RaceStateMachine.Awake() called", LogType.Log, "StateMachine");

        // Singleton pattern с проверкой дубликатов
        if (Instance != null && Instance != this)
        {
            WebGLDebugLogger.Log($"Duplicate RaceStateMachine found! Destroying {gameObject.name}", LogType.Warning, "StateMachine");
            WebGLDebugLogger.SetSystemState("StateMachine_Duplicate", true);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        WebGLDebugLogger.Log($"RaceStateMachine initialized successfully. Initial state: {Current}", LogType.Log, "StateMachine");
        WebGLDebugLogger.SetSystemState("StateMachine", true);
        WebGLDebugLogger.SetSystemValue("CurrentState", Current.ToString());
        WebGLDebugLogger.SetSystemValue("StateMachine_GameObject", gameObject.name);

        // Начинаем корутину мониторинга для WebGL
        StartCoroutine(MonitoringCoroutine());
    }

    private void OnDestroy()
    {
        WebGLDebugLogger.Log("RaceStateMachine.OnDestroy() called", LogType.Log, "StateMachine");

        if (Instance == this)
        {
            Instance = null;
            WebGLDebugLogger.SetSystemState("StateMachine", false);
            WebGLDebugLogger.Log("RaceStateMachine Instance cleared", LogType.Log, "StateMachine");
        }
    }

    /// <summary>
    /// WebGL-совместимое изменение состояния с дополнительными проверками
    /// </summary>
    public void ChangeState(RaceState newState)
    {
        if (_verboseLogging)
        {
            WebGLDebugLogger.Log($"ChangeState called: {Current} -> {newState}", LogType.Log, "StateMachine");
        }

        if (Current == newState)
        {
            WebGLDebugLogger.Log($"State {newState} already active, skipping", LogType.Log, "StateMachine");
            return;
        }

        var previousState = Current;
        Current = newState;

        // Обновляем системные значения
        WebGLDebugLogger.SetSystemValue("CurrentState", newState.ToString());
        WebGLDebugLogger.SetSystemValue("PreviousState", previousState.ToString());
        WebGLDebugLogger.LogStateTransition(previousState.ToString(), newState.ToString(), "StateMachine");

        // Вызываем события через корутину для WebGL стабильности
        StartCoroutine(NotifyStateChangeCoroutine(newState, previousState));
    }

    /// <summary>
    /// WebGL-совместимое уведомление о смене состояния
    /// </summary>
    private IEnumerator NotifyStateChangeCoroutine(RaceState newState, RaceState previousState)
    {
        // Даем кадр для стабилизации состояния
        yield return null;

        try
        {
            if (_verboseLogging)
            {
                WebGLDebugLogger.Log($"Notifying {_subscribersCount} subscribers about state change", LogType.Log, "StateMachine");
            }

            OnStateChanged?.Invoke(newState);
            WebGLDebugLogger.IncrementEventCount("StateChanged");

            WebGLDebugLogger.Log($"State notification completed: {previousState} → {newState}", LogType.Log, "StateMachine");
        }
        catch (System.Exception ex)
        {
            WebGLDebugLogger.Log($"Error during state change notification: {ex.Message}", LogType.Error, "StateMachine");
        }
    }

    /// <summary>
    /// Мониторинг системы для WebGL отладки
    /// </summary>
    private IEnumerator MonitoringCoroutine()
    {
        while (this != null)
        {
            yield return new WaitForSeconds(1f); // Проверяем каждую секунду

            // Подсчитываем подписчиков
            _subscribersCount = OnStateChanged?.GetInvocationList()?.Length ?? 0;
            WebGLDebugLogger.SetSystemValue("StateChangeSubscribers", _subscribersCount.ToString());

            // Проверяем целостность Instance
            if (Instance != this)
            {
                WebGLDebugLogger.Log("Instance integrity check failed!", LogType.Error, "StateMachine");
                WebGLDebugLogger.SetSystemState("StateMachine_Integrity", false);
            }
            else
            {
                WebGLDebugLogger.SetSystemState("StateMachine_Integrity", true);
            }

            // Проверяем активность GameObject
            if (!gameObject.activeInHierarchy)
            {
                WebGLDebugLogger.Log("StateMachine GameObject is inactive!", LogType.Warning, "StateMachine");
                WebGLDebugLogger.SetSystemState("StateMachine_Active", false);
            }
            else
            {
                WebGLDebugLogger.SetSystemState("StateMachine_Active", true);
            }
        }
    }

    /// <summary>
    /// Публичный метод для подписки на события с логированием
    /// </summary>
    public void SubscribeToStateChanges(System.Action<RaceState> callback, string subscriberName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log($"Null callback from {subscriberName}", LogType.Warning, "StateMachine");
            return;
        }

        OnStateChanged += callback;
        WebGLDebugLogger.Log($"'{subscriberName}' subscribed to state changes", LogType.Log, "StateMachine");
        WebGLDebugLogger.IncrementEventCount("StateChangeSubscription");
    }

    /// <summary>
    /// Публичный метод для отписки от событий с логированием
    /// </summary>
    public void UnsubscribeFromStateChanges(System.Action<RaceState> callback, string subscriberName = "Unknown")
    {
        if (callback == null)
        {
            WebGLDebugLogger.Log($"Null callback from {subscriberName}", LogType.Warning, "StateMachine");
            return;
        }

        OnStateChanged -= callback;
        WebGLDebugLogger.Log($"'{subscriberName}' unsubscribed from state changes", LogType.Log, "StateMachine");
        WebGLDebugLogger.IncrementEventCount("StateChangeUnsubscription");
    }

    /// <summary>
    /// WebGL-совместимый метод получения информации о состоянии
    /// </summary>
    public string GetDebugInfo()
    {
        return $"State: {Current}, Subscribers: {_subscribersCount}, Instance Valid: {Instance == this}";
    }

    /// <summary>
    /// Принудительная проверка состояния для отладки
    /// </summary>
    [ContextMenu("Force State Check")]
    public void ForceStateCheck()
    {
        WebGLDebugLogger.Log($"Force state check - Current: {Current}, Subscribers: {_subscribersCount}", LogType.Log, "StateMachine");
        WebGLDebugLogger.SetSystemValue("LastStateCheck", System.DateTime.Now.ToString("HH:mm:ss"));
    }

    /// <summary>
    /// WebGL-совместимая проверка валидности состояния
    /// </summary>
    public bool IsStateValid()
    {
        bool isValid = Instance == this && gameObject != null && gameObject.activeInHierarchy;
        WebGLDebugLogger.SetSystemState("StateMachine_Valid", isValid);
        return isValid;
    }
}