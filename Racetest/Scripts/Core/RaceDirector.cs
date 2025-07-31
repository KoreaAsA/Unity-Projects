// Core/RaceDirector.cs
using System.Collections;
using UnityEngine;

public sealed class RaceDirector : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private LapTimer _lapTimer;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GhostTrackStorage _trackStorage;
    [SerializeField] private RaceUI _ui;
    [SerializeField] private float _pauseAfterFinish = 3f;

    private GameObject _playerCar;
    private GameObject _ghostCar;

    private void Awake()
    {
        var state = RaceStateMachine.Instance;
        state.OnStateChanged += s => Time.timeScale = s == RaceState.Paused ? 0f : 1f;

        _lapTimer.OnLapCompleted += OnLapCompleted;
        _ui.OnStartClicked += StartRace;
        _ui.OnRetryClicked += ResetRace;
    }

    private void Start() => RaceStateMachine.Instance.ChangeState(RaceState.Idle);

    private void StartRace() => StartCoroutine(RaceFlow());

    private IEnumerator RaceFlow()
    {
        RaceStateMachine.Instance.ChangeState(RaceState.Countdown);
        yield return _ui.CountdownSequence();          // 3-2-1

        RaceStateMachine.Instance.ChangeState(RaceState.Racing);

        _trackStorage.Clear();
        _playerCar = _spawnManager.SpawnPlayer();
        _playerCar.GetComponent<Recorder>().enabled = true;

        if (_trackStorage.Frames.Count > 0)
        {
            _ghostCar = _spawnManager.SpawnGhost();
            var ghost = _ghostCar.GetComponent<GhostDriver>();
            ghost.Load(_trackStorage.Frames);
            ghost.StartPlayback();
        }
    }

    private void OnLapCompleted(float lapTime)
    {
        RaceStateMachine.Instance.ChangeState(RaceState.Finished);
        foreach (var f in _playerCar.GetComponent<Recorder>().GetTrajectory())
            _trackStorage.Add(f);

        _ui.ShowResult(lapTime);
        StartCoroutine(PauseThenPaused());
    }

    private IEnumerator PauseThenPaused()
    {
        yield return new WaitForSecondsRealtime(_pauseAfterFinish);
        RaceStateMachine.Instance.ChangeState(RaceState.Paused);
    }

    private void ResetRace()
    {
        if (_playerCar != null) Destroy(_playerCar);
        if (_ghostCar != null) Destroy(_ghostCar);
        _trackStorage.Clear();
        RaceStateMachine.Instance.ChangeState(RaceState.Idle);
    }
}