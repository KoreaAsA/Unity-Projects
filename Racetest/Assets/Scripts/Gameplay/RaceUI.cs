
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class RaceUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _idlePanel;
    [SerializeField] private GameObject _racePanel;
    [SerializeField] private GameObject _resultPanel;

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _retryButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _countdownText;

    public event System.Action OnStartClicked;
    public event System.Action OnRetryClicked;

    [ContextMenu("Check Logger")]
    void CheckLogger()
    {
        var logger = FindObjectOfType<WebGLDebugLogger>();
        Debug.Log($"Logger in scene: {logger != null}");
        Debug.Log($"Logger instance: {WebGLDebugLogger.Instance != null}");

        if (logger != null)
        {
            Debug.Log($"Logger GameObject: {logger.gameObject.name}");
            Debug.Log($"DontDestroyOnLoad: {logger.gameObject.scene.name}");
        }
    }

    private void Start()
    {

        CheckLogger(); 

        _idlePanel.SetActive(true);
        _racePanel.SetActive(false);
        _resultPanel.SetActive(false);
    }

    private void Awake()
    {

        _idlePanel.SetActive(true);
        _racePanel.SetActive(true);
        _resultPanel.SetActive(true);

        // Подписываемся на кнопки
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(() => {
                Debug.Log("[RaceUI] Start button clicked");
                OnStartClicked?.Invoke();
            });
        }

        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(() => {
                Debug.Log("[RaceUI] Retry button clicked");
                OnRetryClicked?.Invoke();
            });
        }

        Debug.Log("[RaceUI] UI initialized successfully");
    }

    public void ShowIdle()
    {
        Debug.Log("[RaceUI] Showing Idle panel (first race)");
        SetPanelStates(idle: true, race: false, result: false);
        SetCountdownVisible(false);

        // В первой гонке показываем кнопку Start, скрываем Retry
        SetButtonStates(showStart: true, showRetry: false);
    }

    // Показать только кнопку Retry (для последующих гонок)
    public void ShowRetryOnly()
    {
        Debug.Log("[RaceUI] Showing Retry only panel (subsequent race)");
        SetPanelStates(idle: true, race: false, result: false);
        SetCountdownVisible(false);

        // Показываем только кнопку Retry, скрываем Start
        SetButtonStates(showStart: false, showRetry: true);
    }

    public void ShowCountdown()
    {
        Debug.Log("[RaceUI] Showing Countdown panel");
        SetPanelStates(idle: false, race: true, result: false);
        SetCountdownVisible(true);

        // Во время отсчета скрываем все кнопки
        SetButtonStates(showStart: false, showRetry: false);
    }

    public void ShowRace()
    {
        Debug.Log("[RaceUI] Showing Race panel");
        SetPanelStates(idle: false, race: true, result: false);
        SetCountdownVisible(false);

        // Во время гонки скрываем все кнопки
        SetButtonStates(showStart: false, showRetry: false);
    }

    public void ShowResult(float time)
    {
        Debug.Log($"[RaceUI] Showing Result panel with time: {time:F2}s");

        if (_timeText != null)
        {
            _timeText.text = $"Время: {time:F2} сек";
        }

        SetPanelStates(idle: false, race: false, result: true);

        // В результатах показываем только кнопку Retry
        SetButtonStates(showStart: false, showRetry: true);
    }

    public IEnumerator CountdownSequence()
    {
        Debug.Log("[RaceUI] Starting countdown sequence");
        ShowCountdown();

        for (int i = 3; i > 0; i--)
        {
            if (_countdownText != null)
            {
                _countdownText.text = i.ToString();
                Debug.Log($"[RaceUI] Countdown: {i}");
            }
            yield return new WaitForSecondsRealtime(1f);
        }

        SetCountdownVisible(false);
        Debug.Log("[RaceUI] Countdown sequence completed");
    }

    private void SetPanelStates(bool idle, bool race, bool result)
    {
        if (_idlePanel != null) _idlePanel.SetActive(idle);
        if (_racePanel != null) _racePanel.SetActive(race);
        if (_resultPanel != null) _resultPanel.SetActive(result);
    }

    // Управление видимостью кнопок
    private void SetButtonStates(bool showStart, bool showRetry)
    {
        if (_startButton != null && _startButton.gameObject != null)
        {
            _startButton.gameObject.SetActive(showStart);
        }

        if (_retryButton != null && _retryButton.gameObject != null)
        {
            _retryButton.gameObject.SetActive(showRetry);
        }

        Debug.Log($"[RaceUI] Button states - Start: {showStart}, Retry: {showRetry}");
    }

    private void SetCountdownVisible(bool visible)
    {
        if (_countdownText != null && _countdownText.gameObject != null)
        {
            _countdownText.gameObject.SetActive(visible);
        }
    }
}