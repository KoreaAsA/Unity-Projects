using UnityEngine;

public sealed class RaceManager : MonoBehaviour
{
    [SerializeField] private GhostTrackSO _track;
    [SerializeField] private SpawnManager _spawn;
    [SerializeField] private LapTimer _timer;
    [SerializeField] private RaceUI _ui;

    private GameObject _player;
    private GameObject _ghost;

    private void Awake()
    {
        _timer.OnLapCompleted += HandleLapCompleted;
        _ui.OnStartClicked += HandleStartClicked;
        _ui.OnRetryClicked += HandleRetryClicked;
    }

    private void Start() => _ui.ShowIdle();

    private void HandleStartClicked()
    {
        _track.Clear();
        _timer.StartTimer();

        _player = _spawn.SpawnPlayer();
        _player.GetComponent<Recorder>().enabled = true;

        _ui.ShowRace();
    }

    private void HandleLapCompleted(float time)
    {
        _player.GetComponent<Recorder>().enabled = false;
        _track.Clear();
        foreach (var f in _player.GetComponent<Recorder>().GetTrajectory())
            _track.Add(f);

        if (_ghost != null) Destroy(_ghost);
        _ghost = _spawn.SpawnGhost();
        var driver = _ghost.GetComponent<GhostDriver>();
        driver.Load(_track.Frames);
        driver.StartPlayback();

        _ui.ShowResult(time);
    }

    private void HandleRetryClicked()
    {
        if (_player != null) Destroy(_player);
        if (_ghost != null) Destroy(_ghost);
        _track.Clear();
        _ui.ShowIdle();
    }
}