using UnityEngine;
using System.Collections;

/// <summary>
/// WebGL-адаптированный Bootstrap с улучшенным логированием
/// </summary>
public sealed class Bootstrap : MonoBehaviour
{
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;
    [SerializeField] private CountdownFinishedSignal _countdownFinishedSignal;

    [Header("WebGL Settings")]
    [SerializeField] private bool _enableWebGLOptimizations = true;
    [SerializeField] private int _targetFrameRate = 60;

    [Header("Execution Order")]
    [SerializeField] private int _executionOrder = -100;

    private void Awake()
    {
        // Инициализируем логгер в первую очередь
        var logger = WebGLDebugLogger.Instance;

        WebGLDebugLogger.Log("Starting game initialization...", LogType.Log, "Bootstrap");
        WebGLDebugLogger.SetSystemState("Bootstrap", true);

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

        WebGLDebugLogger.Log("WebGL optimizations applied", LogType.Log, "Bootstrap");
        WebGLDebugLogger.SetSystemValue("TargetFPS", _targetFrameRate.ToString());
    }

    private IEnumerator InitializeStateMachine()
    {
        // Даем кадр на инициализацию других систем
        yield return null;

        WebGLDebugLogger.Log("Initializing RaceStateMachine...", LogType.Log, "Bootstrap");

        // Создаём RaceStateMachine, если его ещё нет
        if (RaceStateMachine.Instance == null)
        {
            WebGLDebugLogger.Log("Creating new RaceStateMachine instance", LogType.Log, "Bootstrap");

            var rsmObject = new GameObject("RaceStateMachine");
            var rsm = rsmObject.AddComponent<RaceStateMachine>();

            // Убедимся что объект не уничтожится при загрузке новой сцены
            DontDestroyOnLoad(rsmObject);

            WebGLDebugLogger.Log("RaceStateMachine created successfully", LogType.Log, "Bootstrap");
            WebGLDebugLogger.SetSystemState("RaceStateMachine", true);
        }
        else
        {
            WebGLDebugLogger.Log("RaceStateMachine already exists", LogType.Warning, "Bootstrap");
            WebGLDebugLogger.SetSystemState("RaceStateMachine", true);
        }

        // Ждем еще кадр для полной инициализации
        yield return null;

        // Проверяем что Instance действительно доступен
        if (RaceStateMachine.Instance == null)
        {
            WebGLDebugLogger.Log("RaceStateMachine.Instance is still null after creation!", LogType.Error, "Bootstrap");
            WebGLDebugLogger.SetSystemState("RaceStateMachine", false);
            yield break;
        }

        // Устанавливаем начальное состояние
        RaceStateMachine.Instance.ChangeState(RaceState.Idle);
        WebGLDebugLogger.LogStateTransition("None", "Idle", "Bootstrap");

        // Валидируем сигналы
        yield return StartCoroutine(ValidateSignalsCoroutine());

        WebGLDebugLogger.Log("Game systems initialized successfully", LogType.Log, "Bootstrap");
        WebGLDebugLogger.SetSystemState("GameInitialized", true);
    }

    private IEnumerator ValidateSignalsCoroutine()
    {
        WebGLDebugLogger.Log("Starting signal validation...", LogType.Log, "Bootstrap");

        yield return null; // Даем кадр для WebGL

        bool allValid = true;

        // Проверяем RaceStartedSignal
        if (_raceStartedSignal == null)
        {
            WebGLDebugLogger.Log("RaceStartedSignal not assigned!", LogType.Error, "Bootstrap");
            WebGLDebugLogger.SetSystemState("RaceStartedSignal", false);
            allValid = false;
        }
        else
        {
            WebGLDebugLogger.SetSystemState("RaceStartedSignal", true);
        }

        // Проверяем RaceFinishedSignal
        if (_raceFinishedSignal == null)
        {
            WebGLDebugLogger.Log("RaceFinishedSignal not assigned!", LogType.Error, "Bootstrap");
            WebGLDebugLogger.SetSystemState("RaceFinishedSignal", false);
            allValid = false;
        }
        else
        {
            WebGLDebugLogger.SetSystemState("RaceFinishedSignal", true);
        }

        // Проверяем CountdownFinishedSignal
        if (_countdownFinishedSignal == null)
        {
            WebGLDebugLogger.Log("CountdownFinishedSignal not assigned!", LogType.Error, "Bootstrap");
            WebGLDebugLogger.SetSystemState("CountdownFinishedSignal", false);
            allValid = false;
        }
        else
        {
            WebGLDebugLogger.SetSystemState("CountdownFinishedSignal", true);
        }

        if (allValid)
        {
            WebGLDebugLogger.Log("All signals validated successfully", LogType.Log, "Bootstrap");
            WebGLDebugLogger.SetSystemState("SignalsValidated", true);
        }
        else
        {
            WebGLDebugLogger.Log("Some signals are missing - create them via Create menu", LogType.Warning, "Bootstrap");
            WebGLDebugLogger.SetSystemState("SignalsValidated", false);
        }

        WebGLDebugLogger.SetSystemValue("ValidatedSignals", $"{(allValid ? "All" : "Some missing")}");
    }

    private void OnDestroy()
    {
        WebGLDebugLogger.Log("Bootstrap destroyed", LogType.Log, "Bootstrap");
        WebGLDebugLogger.SetSystemState("Bootstrap", false);
    }

    // WebGL-совместимый метод для проверки производительности
    private void Update()
    {
        // Мониторим FPS только в debug режиме для WebGL
        if (Time.frameCount % 60 == 0) // Каждые 60 кадров
        {
            float fps = 1f / Time.unscaledDeltaTime;
            WebGLDebugLogger.SetSystemValue("CurrentFPS", Mathf.RoundToInt(fps).ToString());

            // Предупреждаем о низком FPS
            if (fps < _targetFrameRate * 0.8f)
            {
                WebGLDebugLogger.Log($"Low FPS detected: {fps:F1}", LogType.Warning, "Performance");
            }
        }
    }
}