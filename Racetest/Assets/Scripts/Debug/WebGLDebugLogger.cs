using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// WebGL-совместимая система логирования с OnGUI интерфейсом для отслеживания состояний системы
/// </summary>
public sealed class WebGLDebugLogger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool _showDebugGUI = true;
    [SerializeField] private bool _logToConsole = true;
    [SerializeField] private int _maxLogEntries = 50;
    [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

    private static WebGLDebugLogger _instance;
    public static WebGLDebugLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[WebGL Debug Logger]");
                _instance = go.AddComponent<WebGLDebugLogger>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Структура для логов
    [System.Serializable]
    public struct LogEntry
    {
        public string message;
        public LogType type;
        public float timestamp;
        public string category;

        public LogEntry(string msg, LogType logType, string cat = "General")
        {
            message = msg;
            type = logType;
            timestamp = Time.time;
            category = cat;
        }
    }

    // Коллекции для отслеживания состояний
    private readonly List<LogEntry> _logs = new List<LogEntry>();
    private readonly Dictionary<string, bool> _systemStates = new Dictionary<string, bool>();
    private readonly Dictionary<string, string> _systemValues = new Dictionary<string, string>();
    private readonly Dictionary<string, int> _eventCounts = new Dictionary<string, int>();

    private Vector2 _scrollPosition;
    private bool _guiInitialized = false;
    private GUIStyle _headerStyle;
    private GUIStyle _logStyle;
    private GUIStyle _errorStyle;
    private GUIStyle _warningStyle;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWebGLCompatible();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWebGLCompatible()
    {
        // WebGL-специфичная инициализация
        Application.targetFrameRate = 60;

        // Отключаем сборщик мусора на каждом кадре для WebGL
        QualitySettings.vSyncCount = 0;

        Log("WebGL Debug Logger initialized", LogType.Log, "System");
        SetSystemState("DebugLogger", true);
    }

    private void Update()
    {
        // WebGL-совместимая проверка ввода
        if (Input.GetKeyDown(_toggleKey))
        {
            _showDebugGUI = !_showDebugGUI;
            Log($"Debug GUI toggled: {_showDebugGUI}", LogType.Log, "UI");
        }

        // Обновляем системные значения
        UpdateSystemValues();
    }

    private void UpdateSystemValues()
    {
        SetSystemValue("FPS", Mathf.RoundToInt(1f / Time.unscaledDeltaTime).ToString());
        SetSystemValue("Time", Time.time.ToString("F1"));

        if (RaceStateMachine.Instance != null)
        {
            SetSystemValue("RaceState", RaceStateMachine.Instance.Current.ToString());
            SetSystemState("StateMachine", true);
        }
        else
        {
            SetSystemState("StateMachine", false);
        }
    }

    #region Public API

    public static void Log(string message, LogType type = LogType.Log, string category = "General")
    {
        var logger = Instance;
        if (logger == null) return;

        var entry = new LogEntry(message, type, category);
        logger._logs.Add(entry);

        // Ограничиваем количество логов для WebGL
        if (logger._logs.Count > logger._maxLogEntries)
        {
            logger._logs.RemoveAt(0);
        }

        if (logger._logToConsole)
        {
            string formattedMessage = $"[{category}] {message}";
            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(formattedMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                default:
                    Debug.Log(formattedMessage);
                    break;
            }
        }
    }

    public static void SetSystemState(string systemName, bool isActive)
    {
        var logger = Instance;
        if (logger == null) return;

        logger._systemStates[systemName] = isActive;
        Log($"System '{systemName}' state: {(isActive ? "ACTIVE" : "INACTIVE")}",
            isActive ? LogType.Log : LogType.Warning, "SystemState");
    }

    public static void SetSystemValue(string key, string value)
    {
        var logger = Instance;
        if (logger == null) return;

        logger._systemValues[key] = value;
    }

    public static void IncrementEventCount(string eventName)
    {
        var logger = Instance;
        if (logger == null) return;

        if (logger._eventCounts.ContainsKey(eventName))
            logger._eventCounts[eventName]++;
        else
            logger._eventCounts[eventName] = 1;

        Log($"Event '{eventName}' fired (count: {logger._eventCounts[eventName]})", LogType.Log, "Events");
    }

    public static void LogStateTransition(string from, string to, string system = "StateMachine")
    {
        Log($"State transition: {from} → {to}", LogType.Log, system);
    }

    public static void LogSignalFired(string signalName, string data = "")
    {
        IncrementEventCount(signalName);
        string message = string.IsNullOrEmpty(data) ?
            $"Signal fired: {signalName}" :
            $"Signal fired: {signalName} ({data})";
        Log(message, LogType.Log, "Signals");
    }

    #endregion

    #region OnGUI Implementation

    private void OnGUI()
    {
        if (!_showDebugGUI) return;

        InitializeGUIStyles();
        DrawDebugWindow();
    }

    private void InitializeGUIStyles()
    {
        if (_guiInitialized) return;

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _logStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            normal = { textColor = Color.white },
            wordWrap = true
        };

        _errorStyle = new GUIStyle(_logStyle)
        {
            normal = { textColor = Color.red }
        };

        _warningStyle = new GUIStyle(_logStyle)
        {
            normal = { textColor = Color.yellow }
        };

        _guiInitialized = true;
    }

    private void DrawDebugWindow()
    {
        float windowWidth = Screen.width * 0.4f;
        float windowHeight = Screen.height * 0.8f;

        GUILayout.BeginArea(new Rect(10, 10, windowWidth, windowHeight), GUI.skin.box);

        GUILayout.Label("WebGL Debug Console", _headerStyle);
        GUILayout.Space(5);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(windowHeight - 100));

        // Системные состояния
        DrawSystemStates();
        GUILayout.Space(10);

        // Системные значения
        DrawSystemValues();
        GUILayout.Space(10);

        // Счетчики событий
        DrawEventCounts();
        GUILayout.Space(10);

        // Логи
        DrawLogs();

        GUILayout.EndScrollView();

        // Кнопки управления
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Logs"))
        {
            _logs.Clear();
        }
        if (GUILayout.Button("Clear Counters"))
        {
            _eventCounts.Clear();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawSystemStates()
    {
        GUILayout.Label("System States:", _headerStyle);

        if (_systemStates.Count == 0)
        {
            GUILayout.Label("No systems registered", _logStyle);
            return;
        }

        foreach (var kvp in _systemStates)
        {
            GUIStyle style = kvp.Value ? _logStyle : _errorStyle;
            string status = kvp.Value ? "✓" : "✗";
            GUILayout.Label($"{status} {kvp.Key}: {(kvp.Value ? "ACTIVE" : "INACTIVE")}", style);
        }
    }

    private void DrawSystemValues()
    {
        GUILayout.Label("System Values:", _headerStyle);

        if (_systemValues.Count == 0)
        {
            GUILayout.Label("No values tracked", _logStyle);
            return;
        }

        foreach (var kvp in _systemValues)
        {
            GUILayout.Label($"{kvp.Key}: {kvp.Value}", _logStyle);
        }
    }

    private void DrawEventCounts()
    {
        GUILayout.Label("Event Counters:", _headerStyle);

        if (_eventCounts.Count == 0)
        {
            GUILayout.Label("No events fired yet", _logStyle);
            return;
        }

        foreach (var kvp in _eventCounts)
        {
            GUILayout.Label($"{kvp.Key}: {kvp.Value}", _logStyle);
        }
    }

    private void DrawLogs()
    {
        GUILayout.Label("Recent Logs:", _headerStyle);

        if (_logs.Count == 0)
        {
            GUILayout.Label("No logs yet", _logStyle);
            return;
        }

        // Показываем логи в обратном порядке (новые сверху)
        for (int i = _logs.Count - 1; i >= 0; i--)
        {
            var log = _logs[i];
            GUIStyle style = GetLogStyle(log.type);

            string timeStr = log.timestamp.ToString("F1");
            string message = $"[{timeStr}s] [{log.category}] {log.message}";

            GUILayout.Label(message, style);
        }
    }

    private GUIStyle GetLogStyle(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                return _errorStyle;
            case LogType.Warning:
                return _warningStyle;
            default:
                return _logStyle;
        }
    }

    #endregion
}