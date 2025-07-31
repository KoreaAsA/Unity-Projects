using UnityEngine;
using System.Collections;

public sealed class RaceUIPresenter : MonoBehaviour
{
    [SerializeField] private RaceUI _ui;

    // ИЗМЕНЕНИЕ: Добавляем флаг для отслеживания первой гонки
    private bool _isFirstRace = true;

    private void Start()
    {
        // Используем Start вместо OnEnable для гарантии что Bootstrap уже выполнился
        StartCoroutine(WaitForStateMachineAndInitialize());
    }

    private IEnumerator WaitForStateMachineAndInitialize()
    {
        // Ждем пока RaceStateMachine не будет создан
        while (RaceStateMachine.Instance == null)
        {
            Debug.Log("[RaceUIPresenter] Waiting for RaceStateMachine to be created...");
            yield return null;
        }

        Debug.Log("[RaceUIPresenter] RaceStateMachine found, initializing UI presenter");

        // Проверяем что UI назначен
        if (_ui == null)
        {
            Debug.LogError("[RaceUIPresenter] RaceUI component not assigned!");
            yield break;
        }

        // Подписываемся на изменения состояния
        var stateMachine = RaceStateMachine.Instance;
        stateMachine.OnStateChanged += OnStateChanged;

        // ИЗМЕНЕНИЕ: Подписываемся на события UI для отслеживания retry
        _ui.OnRetryClicked += OnRetryClicked;

        // Показываем текущее состояние сразу
        OnStateChanged(stateMachine.Current);

        Debug.Log($"[RaceUIPresenter] Initialized successfully. Current state: {stateMachine.Current}");
    }

    private void OnEnable()
    {
        // Если StateMachine уже существует, подписываемся сразу
        if (RaceStateMachine.Instance != null)
        {
            Debug.Log("[RaceUIPresenter] StateMachine available in OnEnable, subscribing immediately");
            RaceStateMachine.Instance.OnStateChanged += OnStateChanged;
            OnStateChanged(RaceStateMachine.Instance.Current);
        }
    }

    private void OnDisable()
    {
        // Отписываемся только если Instance не null
        if (RaceStateMachine.Instance != null)
        {
            RaceStateMachine.Instance.OnStateChanged -= OnStateChanged;
        }

        // ИЗМЕНЕНИЕ: Отписываемся от UI событий
        if (_ui != null)
        {
            _ui.OnRetryClicked -= OnRetryClicked;
        }

        Debug.Log("[RaceUIPresenter] Unsubscribed from state changes");
    }

    // НОВЫЙ МЕТОД: Обработка нажатия Retry
    private void OnRetryClicked()
    {
        _isFirstRace = false;
        Debug.Log("[RaceUIPresenter] Retry clicked - subsequent race");
    }

    private void OnStateChanged(RaceState state)
    {
        if (_ui == null)
        {
            Debug.LogError("[RaceUIPresenter] RaceUI is null in OnStateChanged!");
            return;
        }

        Debug.Log($"[RaceUIPresenter] Handling state change: {state} (FirstRace: {_isFirstRace})");

        switch (state)
        {
            case RaceState.Idle:
                // ИЗМЕНЕНИЕ: Показываем разный UI в зависимости от того, первая это гонка или нет
                if (_isFirstRace)
                {
                    _ui.ShowIdle(); // Показываем кнопку "Start"
                }
                else
                {
                    _ui.ShowRetryOnly(); // Показываем только кнопку "Retry"
                }
                break;

            case RaceState.Countdown:
                _ui.ShowCountdown();
                break;

            case RaceState.Racing:
                _ui.ShowRace();
                break;

            case RaceState.Finished:
                // Результат покажет RaceFinishedSignal через RaceDirector
                Debug.Log("[RaceUIPresenter] Race finished - result will be shown by RaceDirector");
                break;

            case RaceState.Paused:
                Debug.Log("[RaceUIPresenter] Game paused");
                break;

            default:
                Debug.LogWarning($"[RaceUIPresenter] Unhandled state: {state}");
                break;
        }
    }
}