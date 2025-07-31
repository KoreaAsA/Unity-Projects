// Assets/_Project/Scripts/UI/RaceUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class RaceUI : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject _idlePanel;
    [SerializeField] private GameObject _racePanel;
    [SerializeField] private GameObject _resultPanel;

    [Header("Кнопки")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _retryButton;

    [Header("Тексты")]
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _countdownText;

    public event System.Action OnStartClicked;
    public event System.Action OnRetryClicked;

    private void Awake()
    {
        _startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
    }

    private void Start()
    {
        // все панели выключены – включит RaceDirector при Idle
    }

    public void ShowIdle()
    {
        _idlePanel.SetActive(true);
        _racePanel.SetActive(false);
        _resultPanel.SetActive(false);
        _countdownText.gameObject.SetActive(false);
    }

    public void ShowCountdown()
    {
        _idlePanel.SetActive(false);
        _racePanel.SetActive(true);
        _resultPanel.SetActive(false);
    }

    public void ShowRace()
    {
        _idlePanel.SetActive(false);
        _racePanel.SetActive(true);
        _resultPanel.SetActive(false);
        _countdownText.gameObject.SetActive(false);
    }

    public void ShowResult(float time)
    {
        _timeText.text = $"Время: {time:F2} сек";
        _idlePanel.SetActive(false);
        _racePanel.SetActive(false);
        _resultPanel.SetActive(true);
    }

    public IEnumerator CountdownSequence()
    {
        ShowCountdown();
        _countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }
        _countdownText.gameObject.SetActive(false);
    }
}