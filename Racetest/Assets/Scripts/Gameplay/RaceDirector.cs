using System.Collections;
using UnityEngine;

public sealed class RaceDirector : MonoBehaviour
{
    [Header("Signals")]
    [SerializeField] private CountdownFinishedSignal _countdownFinishedSignal;
    [SerializeField] private RaceStartedSignal _raceStartedSignal;
    [SerializeField] private RaceFinishedSignal _raceFinishedSignal;

    [Header("Refs")]
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GhostTrackStorage _trackStorage;
    [SerializeField] private RaceUI _raceUI;

    private GameObject _playerCar;
    private GameObject _ghostCar;
    private Coroutine _raceFlowCoroutine;

    public static bool IsFirstRace { get; private set; } = true;

    private void Start()
    {
        IsFirstRace = true;

        Debug.Log("[RaceDirector] Starting race director...");

            // Проверяем состояние траектории при старте
        if (_trackStorage.IsTrajectoryRecorded)
        {
            Debug.Log($"[RaceDirector] First trajectory already exists with {_trackStorage.Frames.Count} frames");
        }
        else
        {
            Debug.Log("[RaceDirector] No first trajectory yet - will record in this race");
        }

        ValidateComponents();

        if (RaceStateMachine.Instance != null)
        {
            RaceStateMachine.Instance.ChangeState(RaceState.Idle);
        }
        else
        {
            Debug.LogError("[RaceDirector] RaceStateMachine.Instance is null!");
        }
    }

    private void OnEnable()
    {
        if (_raceUI != null)
        {
            _raceUI.OnStartClicked += StartRace;
            _raceUI.OnRetryClicked += RestartRace;
        }

        if (_raceFinishedSignal != null)
        {
            _raceFinishedSignal.AddListener(OnLapCompleted);
        }

        Debug.Log("[RaceDirector] Event subscriptions completed");
    }

    private void OnDisable()
    {
        if (_raceUI != null)
        {
            _raceUI.OnStartClicked -= StartRace;
            _raceUI.OnRetryClicked -= RestartRace;
        }

        if (_raceFinishedSignal != null)
        {
            _raceFinishedSignal.RemoveListener(OnLapCompleted);
        }

        Debug.Log("[RaceDirector] Event unsubscriptions completed");
    }

    private void ValidateComponents()
    {
        bool allValid = true;

        if (_spawnManager == null) { Debug.LogError("[RaceDirector] SpawnManager not assigned!"); allValid = false; }
        if (_trackStorage == null) { Debug.LogError("[RaceDirector] GhostTrackStorage not assigned!"); allValid = false; }
        if (_raceUI == null) { Debug.LogError("[RaceDirector] RaceUI not assigned!"); allValid = false; }
        if (_raceStartedSignal == null) { Debug.LogError("[RaceDirector] RaceStartedSignal not assigned!"); allValid = false; }
        if (_raceFinishedSignal == null) { Debug.LogError("[RaceDirector] RaceFinishedSignal not assigned!"); allValid = false; }
        if (_countdownFinishedSignal == null) { Debug.LogError("[RaceDirector] CountdownFinishedSignal not assigned!"); allValid = false; }

        if (allValid)
        {
            Debug.Log("[RaceDirector] All components validated successfully");
        }
    }

    private void StartRace()
    {
        Debug.Log("[RaceDirector] StartRace called");

        if (_raceFlowCoroutine != null)
        {
            Debug.LogWarning("[RaceDirector] Race already in progress!");
            return;
        }

        if (RaceStateMachine.Instance == null)
        {
            Debug.LogError("[RaceDirector] Cannot start race - RaceStateMachine.Instance is null!");
            return;
        }

        _raceFlowCoroutine = StartCoroutine(RaceFlow());
    }

    private void RestartRace()
    {
        Debug.Log("[RaceDirector] RestartRace called");

        if (_raceFlowCoroutine != null)
        {
            StopCoroutine(_raceFlowCoroutine);
            _raceFlowCoroutine = null;
        }

        CleanupVehicles();
        //IsFirstRace = false; // Устанавливаем что это уже не первая гонка
        RaceStateMachine.Instance.ChangeState(RaceState.Idle);
    }

    private IEnumerator RaceFlow()
    {
        Debug.Log("[RaceDirector] Starting race flow...");

        Debug.Log("[RaceDirector] Phase 1: Setup vehicles");
        bool setupSuccess = SetupRace();
        if (!setupSuccess)
        {
            Debug.LogError("[RaceDirector] Race setup failed!");
            RaceStateMachine.Instance.ChangeState(RaceState.Idle);
            _raceFlowCoroutine = null;
            yield break;
        }

        Debug.Log("[RaceDirector] Phase 2: Starting race immediately");
        RaceStateMachine.Instance.ChangeState(RaceState.Racing);

        _raceStartedSignal.Raise();
        Debug.Log("[RaceDirector] Race started signal raised immediately");

        Debug.Log("[RaceDirector] Phase 3: Waiting for finish");
        yield return new WaitUntil(() => RaceStateMachine.Instance.Current == RaceState.Finished);

        Debug.Log("[RaceDirector] Race flow completed");
        _raceFlowCoroutine = null;
    }

    private bool SetupRace()
    {
        // Используем новый метод проверки траектории
        bool hasGhostData = _trackStorage.IsTrajectoryRecorded;

        // Спавним игрока с учетом первого заезда
        _playerCar = _spawnManager.SpawnPlayer(IsFirstRace);
        if (_playerCar == null)
        {
            Debug.LogError("[RaceDirector] Failed to spawn player car!");
            return false;
        }

        var recorder = _playerCar.GetComponent<Recorder>();
        if (recorder == null)
        {
            Debug.LogError("[RaceDirector] Player car missing Recorder component!");
            return false;
        }

        recorder.Clear();
        recorder.enabled = true;
        Debug.Log("[RaceDirector] Player recorder enabled");

        // Спавним призрака если есть ПЕРВАЯ траектория и это НЕ первый заезд
        if (hasGhostData && !IsFirstRace)
        {
            Debug.Log($"[RaceDirector] Spawning ghost with FIRST trajectory ({_trackStorage.Frames.Count} frames)");
            _ghostCar = _spawnManager.SpawnGhost();
            if (_ghostCar == null)
            {
                Debug.LogError("[RaceDirector] Failed to spawn ghost car!");
                return false;
            }

            var ghost = _ghostCar.GetComponent<SmoothedGhostDriver>();
            if (ghost == null)
            {
                Debug.LogError("[RaceDirector] Ghost car missing SmoothedGhostDriver component!");
                return false;
            }

            // Загружаем ПЕРВУЮ траекторию
            var ghostData = new VehicleSnapshot[_trackStorage.Frames.Count];
            for (int i = 0; i < _trackStorage.Frames.Count; i++)
            {
                ghostData[i] = _trackStorage.Frames[i];
            }

            ghost.Load(ghostData);
            Debug.Log("[RaceDirector] Ghost loaded with FIRST trajectory");
        }
        else if (IsFirstRace)
        {
            Debug.Log("[RaceDirector] First race - no ghost spawned, recording trajectory");
        }
        else
        {
            Debug.Log("[RaceDirector] No first trajectory available");
        }

        return true;
    }
    private void OnLapCompleted(float lapTime)
    {
        Debug.Log($"[RaceDirector] Lap completed in {lapTime:F2} seconds");

        if (_playerCar != null)
        {
            var recorder = _playerCar.GetComponent<Recorder>();
            if (recorder != null)
            {
                var trajectory = recorder.GetTrajectory();

                // Сохраняем траекторию только если это первый заезд
                if (IsFirstRace)
                {
                    bool saved = _trackStorage.TrySaveFirstTrajectory(trajectory);
                    if (saved)
                    {
                        Debug.Log($"[RaceDirector] FIRST trajectory saved with {trajectory.Count} frames");
                    }
                    IsFirstRace = false; // Помечаем что первый заезд завершен
                }
                else
                {
                    Debug.Log($"[RaceDirector] Subsequent race completed, trajectory NOT saved (using first trajectory with {_trackStorage.Frames.Count} frames)");
                }
            }
        }

        RaceStateMachine.Instance.ChangeState(RaceState.Finished);
        _raceUI.ShowResult(lapTime);

        StartCoroutine(DelayedCleanup());
    }

    private IEnumerator DelayedCleanup()
    {
        yield return new WaitForSeconds(1f);
        CleanupVehicles();
    }

    private void CleanupVehicles()
    {
        if (_playerCar != null)
        {
            Debug.Log("[RaceDirector] Destroying player car");
            Destroy(_playerCar);
            _playerCar = null;
        }

        if (_ghostCar != null)
        {
            Debug.Log("[RaceDirector] Destroying ghost car");
            Destroy(_ghostCar);
            _ghostCar = null;
        }
    }
}