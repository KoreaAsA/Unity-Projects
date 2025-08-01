using UnityEngine;
using System.Collections;

public sealed class RaceUIPresenter : MonoBehaviour
{
    [SerializeField] private RaceUI _ui;

    private void Start()
    {
        StartCoroutine(WaitForStateMachineAndInitialize());
    }

    private IEnumerator WaitForStateMachineAndInitialize()
    {
        while (RaceStateMachine.Instance == null)
        {
            Debug.Log("[RaceUIPresenter] Waiting for RaceStateMachine to be created...");
            yield return null;
        }

        Debug.Log("[RaceUIPresenter] RaceStateMachine found, initializing UI presenter");

        if (_ui == null)
        {
            Debug.LogError("[RaceUIPresenter] RaceUI component not assigned!");
            yield break;
        }

        var stateMachine = RaceStateMachine.Instance;
        stateMachine.OnStateChanged += OnStateChanged;

        // Убираем локальный флаг, используем статический из RaceDirector
        _ui.OnRetryClicked += OnRetryClicked;

        OnStateChanged(stateMachine.Current);

        Debug.Log($"[RaceUIPresenter] Initialized successfully. Current state: {stateMachine.Current}");
    }

    private void OnEnable()
    {
        if (RaceStateMachine.Instance != null)
        {
            Debug.Log("[RaceUIPresenter] StateMachine available in OnEnable, subscribing immediately");
            RaceStateMachine.Instance.OnStateChanged += OnStateChanged;
            OnStateChanged(RaceStateMachine.Instance.Current);
        }
    }

    private void OnDisable()
    {
        if (RaceStateMachine.Instance != null)
        {
            RaceStateMachine.Instance.OnStateChanged -= OnStateChanged;
        }

        if (_ui != null)
        {
            _ui.OnRetryClicked -= OnRetryClicked;
        }

        Debug.Log("[RaceUIPresenter] Unsubscribed from state changes");
    }

    private void OnRetryClicked()
    {
        Debug.Log("[RaceUIPresenter] Retry clicked - will be subsequent race");
    }

    private void OnStateChanged(RaceState state)
    {
        if (_ui == null)
        {
            Debug.LogError("[RaceUIPresenter] RaceUI is null in OnStateChanged!");
            return;
        }

        // Используем статический флаг из RaceDirector
        Debug.Log($"[RaceUIPresenter] Handling state change: {state} (FirstRace: {RaceDirector.IsFirstRace})");

        switch (state)
        {
            case RaceState.Idle:
                if (RaceDirector.IsFirstRace)
                {
                    _ui.ShowIdle(); // Показываем кнопку "Start"
                }
                else
                {
                    _ui.ShowIdle(); // Показываем только кнопку "Retry" но пока только Start
                }
                break;

            case RaceState.Countdown:
                _ui.ShowCountdown();
                break;

            case RaceState.Racing:
                _ui.ShowRace();
                break;

            case RaceState.Finished:
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