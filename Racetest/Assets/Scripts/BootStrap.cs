using System;
using UnityEngine;

/// <summary>
/// Исправленный Bootstrap с правильной инициализацией логгера
/// </summary>
public sealed class Bootstrap : MonoBehaviour
{
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;
    [SerializeField] private CountdownFinishedSignal _countdownFinishedSignal;

    [Header("WebGL Settings")]
    [SerializeField] private bool _enableWebGLOptimizations = true;
    [SerializeField] private int _targetFrameRate = 60;

    private void Awake()
    {

        // Теперь безопасно используем логгер
        SafeLog("Starting game initialization...", LogType.Log, "Bootstrap");
        SafeSetSystemState("Bootstrap", true);

        // WebGL оптимизации
        if (_enableWebGLOptimizations)
        {
            ApplyWebGLOptimizations();
        }

        // Создаём RaceStateMachine через корутину для WebGL совместимости
        StartCoroutine(InitializeStateMachine());
    }

    private void ApplyWebGLOptimizations()
    {
        // Устанавливаем целевой FPS для WebGL
        Application.targetFrameRate = _targetFrameRate;

        // Отключаем VSync для лучшей производительности в WebGL
        QualitySettings.vSyncCount = 0;

        // Устанавливаем оптимальные настройки для WebGL
        Time.fixedDeltaTime = 1f / 50f; // 50 Hz для физики

        SafeLog("WebGL optimizations applied", LogType.Log, "Bootstrap");
        SafeSetSystemValue("TargetFPS", _targetFrameRate.ToString());
    }

    private System.Collections.IEnumerator InitializeStateMachine()
    {
        // Даем кадр на инициализацию других систем
        yield return null;

        SafeLog("Initializing RaceStateMachine...", LogType.Log, "Bootstrap");

        // Создаём RaceStateMachine, если его ещё нет
        if (RaceStateMachine.Instance == null)
        {
            SafeLog("Creating new RaceStateMachine instance", LogType.Log, "Bootstrap");

            var rsmObject = new GameObject("RaceStateMachine");
            var rsm = rsmObject.AddComponent<RaceStateMachine>();

            // Убедимся что объект не уничтожится при загрузке новой сцены
            DontDestroyOnLoad(rsmObject);

            SafeLog("RaceStateMachine created successfully", LogType.Log, "Bootstrap");
            SafeSetSystemState("RaceStateMachine", true);
        }
        else
        {
            SafeLog("RaceStateMachine already exists", LogType.Warning, "Bootstrap");
            SafeSetSystemState("RaceStateMachine", true);
        }

        // Ждем еще кадр для полной инициализации
        yield return null;

        // Проверяем что Instance действительно доступен
        if (RaceStateMachine.Instance == null)
        {
            SafeLog("RaceStateMachine.Instance is still null after creation!", LogType.Error, "Bootstrap");
            SafeSetSystemState("RaceStateMachine", false);
            yield break;
        }

        // Устанавливаем начальное состояние
        RaceStateMachine.Instance.ChangeState(RaceState.Idle);
        SafeLogStateTransition("None", "Idle", "Bootstrap");

        // Валидируем сигналы
        yield return StartCoroutine(ValidateSignalsCoroutine());

        SafeLog("Game systems initialized successfully", LogType.Log, "Bootstrap");
        SafeSetSystemState("GameInitialized", true);
    }

    private System.Collections.IEnumerator ValidateSignalsCoroutine()
    {
        SafeLog("Starting signal validation...", LogType.Log, "Bootstrap");

        yield return null; // Даем кадр для WebGL

        bool allValid = true;

        // Проверяем RaceStartedSignal
        if (_raceStartedSignal == null)
        {
            SafeLog("RaceStartedSignal not assigned!", LogType.Error, "Bootstrap");
            SafeSetSystemState("RaceStartedSignal", false);
            allValid = false;
        }
        else
        {
            SafeSetSystemState("RaceStartedSignal", true);
        }

        // Проверяем RaceFinishedSignal
        if (_raceFinishedSignal == null)
        {
            SafeLog("RaceFinishedSignal not assigned!", LogType.Error, "Bootstrap");
            SafeSetSystemState("RaceFinishedSignal", false);
            allValid = false;
        }
        else
        {
            SafeSetSystemState("RaceFinishedSignal", true);
        }

        // Проверяем CountdownFinishedSignal
        if (_countdownFinishedSignal == null)
        {
            SafeLog("CountdownFinishedSignal not assigned!", LogType.Error, "Bootstrap");
            SafeSetSystemState("CountdownFinishedSignal", false);
            allValid = false;
        }
        else
        {
            SafeSetSystemState("CountdownFinishedSignal", true);
        }

        if (allValid)
        {
            SafeLog("All signals validated successfully", LogType.Log, "Bootstrap");
            SafeSetSystemState("SignalsValidated", true);
        }
        else
        {
            SafeLog("Some signals are missing - create them via Create menu", LogType.Warning, "Bootstrap");
            SafeSetSystemState("SignalsValidated", false);
        }

        SafeSetSystemValue("ValidatedSignals", $"{(allValid ? "All" : "Some missing")}");
    }

    private void SafeLog(string message, LogType type = LogType.Log, string category = "General")
    {
        try
        {
            WebGLDebugLogger.Log(message, type, category);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[{category}] {message} (Logger unavailable: {ex.Message})");
        }
    }

    private void SafeSetSystemState(string key, bool value)
    {
        try
        {
            WebGLDebugLogger.SetSystemState(key, value);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Logger SetSystemState error: {ex.Message}");
        }
    }

    private void SafeSetSystemValue(string key, string value)
    {
        try
        {
            if (WebGLDebugLogger.Instance != null)
            {
                WebGLDebugLogger.SetSystemValue(key, value);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Logger SetSystemValue error: {ex.Message}");
        }
    }

    private void SafeLogStateTransition(string from, string to, string system = "StateMachine")
    {
        try
        {
            if (WebGLDebugLogger.Instance != null)
            {
                WebGLDebugLogger.LogStateTransition(from, to, system);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Logger LogStateTransition error: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        SafeLog("Bootstrap destroyed", LogType.Log, "Bootstrap");
        SafeSetSystemState("Bootstrap", false);
    }

    // WebGL-совместимый метод для проверки производительности
    private void Update()
    {
        // Мониторим FPS только в debug режиме для WebGL
        if (Time.frameCount % 60 == 0) // Каждые 60 кадров
        {
            float fps = 1f / Time.unscaledDeltaTime;
            SafeSetSystemValue("CurrentFPS", Mathf.RoundToInt(fps).ToString());

            // Предупреждаем о низком FPS
            if (fps < _targetFrameRate * 0.8f)
            {
                SafeLog($"Low FPS detected: {fps:F1}", LogType.Warning, "Performance");
            }
        }
    }
}