using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class GhostDriver : MonoBehaviour, IPlayback
{
    [SerializeField, Range(0.1f, 3f)] private float _speed = 1f;

    private Rigidbody _rb;
    private IReadOnlyList<VehicleSnapshot> _trajectory;
    private float _startTime;
    private int _index;
    private bool _isRunning;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Load(IReadOnlyList<VehicleSnapshot> t) => _trajectory = t;

    public void SetPlaybackSpeed(float m) => _speed = Mathf.Max(0.1f, m);

    public void StartPlayback()
    {
        if (_trajectory == null || _trajectory.Count == 0)
        {
            Debug.LogWarning("[GhostDriver] No trajectory loaded.");
            return;
        }
        _startTime = Time.time;
        _index = 0;
        _isRunning = true;
    }

    public void StopPlayback() => _isRunning = false;

    private void FixedUpdate()
    {
        if (!_isRunning) return;

        float elapsed = (Time.time - _startTime) * _speed;

        while (_index < _trajectory.Count - 1 && _trajectory[_index + 1].Time <= elapsed)
            _index++;

        var frame = _trajectory[_index];
        _rb.position = frame.Position;
        _rb.rotation = frame.Rotation;
        _rb.velocity = frame.Velocity * _speed;
        _rb.angularVelocity = frame.AngularVelocity * _speed;

        if (_index == _trajectory.Count - 1)
        {
            _isRunning = false;
            Debug.Log("[GhostDriver] Playback finished.");
        }
    }
}

 /*   private void FixedUpdate()
    {
        if (!_running) return;

        // ���� ��������� ���� ����� �� �������
        float targetTime = Time.time + _lookAheadTime;
        while (_idx < _traj.Count - 1 && _traj[_idx].Time < targetTime) _idx++;

        var target = _traj[_idx];
        Vector3 desiredVel = (target.Position - _rb.position).normalized * Mathf.Min(_maxSpeed, target.Velocity.magnitude);
        Vector3 steer = Vector3.ClampMagnitude(desiredVel - _rb.linearVelocity, _acceleration * Time.fixedDeltaTime);
        _rb.AddForce(steer, ForceMode.Acceleration);
    }*/


