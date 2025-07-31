using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;
    [SerializeField] private CountdownFinishedSignal _countdownFinishedSignal;

    [Header("Execution Order")]
    [SerializeField] private int _executionOrder = -100; // Показываем в инспекторе для удобства

    private void Awake()
    {
        Debug.Log("[Bootstrap] ==> Starting game initialization...");

        // Создаём RaceStateMachine, если его ещё нет
        if (RaceStateMachine.Instance == null)
        {
            Debug.Log("[Bootstrap] Creating RaceStateMachine...");

            var rsmObject = new GameObject("RaceStateMachine");
            var rsm = rsmObject.AddComponent<RaceStateMachine>();

            // Убедимся что объект не уничтожится при загрузке новой сцены
            DontDestroyOnLoad(rsmObject);

            Debug.Log("[Bootstrap] RaceStateMachine created successfully");
        }
        else
        {
            Debug.Log("[Bootstrap] RaceStateMachine already exists");
        }

        // Проверяем что Instance действительно доступен
        if (RaceStateMachine.Instance == null)
        {
            Debug.LogError("[Bootstrap] RaceStateMachine.Instance is still null after creation!");
            return;
        }

        // Устанавливаем начальное состояние
        RaceStateMachine.Instance.ChangeState(RaceState.Idle);

        // Валидируем сигналы
        ValidateSignals();

        Debug.Log("[Bootstrap] Game systems initialized successfully");
    }

    private void ValidateSignals()
    {
        bool allValid = true;

        if (_raceStartedSignal == null)
        {
            Debug.LogError("[Bootstrap] RaceStartedSignal not assigned!");
            allValid = false;
        }

        if (_raceFinishedSignal == null)
        {
            Debug.LogError("[Bootstrap] RaceFinishedSignal not assigned!");
            allValid = false;
        }

        if (_countdownFinishedSignal == null)
        {
            Debug.LogError("[Bootstrap] CountdownFinishedSignal not assigned!");
            allValid = false;
        }

        if (allValid)
        {
            Debug.Log("[Bootstrap] All signals validated successfully");
        }
        else
        {
            Debug.LogWarning("[Bootstrap] Some signals are missing - create them via Create menu");
        }
    }

}