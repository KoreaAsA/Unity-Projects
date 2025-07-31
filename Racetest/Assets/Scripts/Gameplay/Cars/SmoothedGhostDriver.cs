using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class SmoothedGhostDriver : MonoBehaviour, IPlayback
{
    [Header("Playback Settings")]
    [SerializeField, Range(0.1f, 3f)] private float _speed = 1f;
    [SerializeField] private RaceStartedSignal _raceStartedSignal;

    [Header("Smoothing")]
    [SerializeField, Range(0.1f, 1f)] private float _smoothingFactor = 0.3f;

    private Rigidbody _rb;
    private VehicleSnapshot[] _trajectory;
    private float _startTime;
    private bool _running;
    private bool _isDataLoaded;
    private int _currentIndex;

    // Для плавного движения
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("[SimpleGhostDriver] Rigidbody component not found!");
        }

        // Изначально делаем кинематическим
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }

        Debug.Log("[SimpleGhostDriver] Initialized");
    }

    public void Load(IReadOnlyList<VehicleSnapshot> trajectory)
    {
        if (trajectory != null && trajectory.Count > 0)
        {
            _trajectory = new VehicleSnapshot[trajectory.Count];
            for (int i = 0; i < trajectory.Count; i++)
            {
                _trajectory[i] = trajectory[i];
            }
            _isDataLoaded = true;
            Debug.Log($"[SimpleGhostDriver] Loaded trajectory with {trajectory.Count} frames");

            SetInitialPosition();
        }
        else
        {
            _trajectory = null;
            _isDataLoaded = false;
            Debug.LogWarning("[SimpleGhostDriver] Empty or null trajectory provided");
        }
    }

    public void Load(VehicleSnapshot[] trajectory)
    {
        _trajectory = trajectory;
        if (trajectory != null && trajectory.Length > 0)
        {
            _isDataLoaded = true;
            Debug.Log($"[SimpleGhostDriver] Loaded trajectory array with {trajectory.Length} frames");
            SetInitialPosition();
        }
        else
        {
            _isDataLoaded = false;
            Debug.LogWarning("[SimpleGhostDriver] Empty or null trajectory array provided");
        }
    }

    private void SetInitialPosition()
    {
        if (_trajectory != null && _trajectory.Length > 0)
        {
            var firstFrame = _trajectory[0];
            transform.position = firstFrame.Position;
            transform.rotation = firstFrame.Rotation;
            _targetPosition = firstFrame.Position;
            _targetRotation = firstFrame.Rotation;
            Debug.Log("[SimpleGhostDriver] Set initial position from trajectory");
        }
    }

    public void SetPlaybackSpeed(float multiplier)
    {
        _speed = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[SimpleGhostDriver] Playback speed set to {_speed}");
    }

    public void StartPlayback()
    {
        if (!_isDataLoaded || _trajectory == null || _trajectory.Length == 0)
        {
            Debug.LogWarning("[SimpleGhostDriver] No trajectory data available for playback");
            return;
        }

        // Переключаем в физический режим для столкновений
        if (_rb != null)
        {
            _rb.isKinematic = false;
        }

        _startTime = Time.time;
        _currentIndex = 0;
        _running = true;

        Debug.Log($"[SimpleGhostDriver] Starting playback with {_trajectory.Length} frames at speed {_speed}x");
    }

    public void StopPlayback()
    {
        _running = false;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[SimpleGhostDriver] Playback stopped and ghost frozen");
    }

    private void OnEnable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.AddListener(StartPlayback);
        }
    }

    private void OnDisable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.RemoveListener(StartPlayback);
        }
    }

    private void FixedUpdate()
    {
        if (!_running || !_isDataLoaded || _trajectory == null || _trajectory.Length == 0 || _rb == null)
            return;

        FollowTrajectory();
    }

    private void FollowTrajectory()
    {
        float elapsed = (Time.time - _startTime) * _speed;

        // Находим нужный кадр в траектории
        while (_currentIndex < _trajectory.Length - 1 && _trajectory[_currentIndex + 1].Time <= elapsed)
        {
            _currentIndex++;
        }

        // Проверяем границы массива
        if (_currentIndex >= _trajectory.Length)
        {
            _running = false;
            Debug.Log("[SimpleGhostDriver] Reached end of trajectory");
            return;
        }

        var currentFrame = _trajectory[_currentIndex];

        // Интерполяция между кадрами для плавности
        if (_currentIndex < _trajectory.Length - 1)
        {
            var nextFrame = _trajectory[_currentIndex + 1];
            float frameProgress = 0f;

            if (nextFrame.Time > currentFrame.Time)
            {
                frameProgress = (elapsed - currentFrame.Time) / (nextFrame.Time - currentFrame.Time);
                frameProgress = Mathf.Clamp01(frameProgress);
            }

            // Интерполируем целевые позицию и поворот
            _targetPosition = Vector3.Lerp(currentFrame.Position, nextFrame.Position, frameProgress);
            _targetRotation = Quaternion.Lerp(currentFrame.Rotation, nextFrame.Rotation, frameProgress);

            // Интерполируем скорости для реалистичной физики
            var targetVelocity = Vector3.Lerp(currentFrame.Velocity, nextFrame.Velocity, frameProgress) * _speed;
            var targetAngularVelocity = Vector3.Lerp(currentFrame.AngularVelocity, nextFrame.AngularVelocity, frameProgress) * _speed;

            // Применяем скорости
            _rb.linearVelocity = targetVelocity;
            _rb.angularVelocity = targetAngularVelocity;
        }
        else
        {
            // Последний кадр - без интерполяции
            _targetPosition = currentFrame.Position;
            _targetRotation = currentFrame.Rotation;
            _rb.linearVelocity = currentFrame.Velocity * _speed;
            _rb.angularVelocity = currentFrame.AngularVelocity * _speed;
        }

        // Плавно двигаем к целевой позиции
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, _targetPosition, _smoothingFactor);
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, _targetRotation, _smoothingFactor);

        // Применяем движение через Rigidbody для физических взаимодействий
        _rb.MovePosition(smoothedPosition);
        _rb.MoveRotation(smoothedRotation);
    }
}