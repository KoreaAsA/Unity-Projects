using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class GhostDriver : MonoBehaviour, IPlayback
{
    [SerializeField, Range(0.1f, 3f)] private float _speed = 1f;
    [SerializeField] private RaceStartedSignal _raceStartedSignal;

    private Rigidbody _rb;
    private IReadOnlyList<VehicleSnapshot> _trajectory;
    private VehicleSnapshot[] _trajectoryArray;
    private float _startTime;
    private int _index;
    private bool _running;
    private bool _isDataLoaded; // ИЗМЕНЕНИЕ: Флаг для проверки загруженности данных

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("[GhostDriver] Rigidbody component not found!");
        }

        // ИЗМЕНЕНИЕ: Замораживаем призрака при создании
        if (_rb != null)
        {
            _rb.isKinematic = true; // Делаем кинематическим до старта гонки
        }

        Debug.Log("[GhostDriver] Initialized (frozen until race starts)");
    }

    public void Load(IReadOnlyList<VehicleSnapshot> trajectory)
    {
        _trajectory = trajectory;

        // Кешируем в массив для лучшей производительности
        if (trajectory != null && trajectory.Count > 0)
        {
            _trajectoryArray = new VehicleSnapshot[trajectory.Count];
            for (int i = 0; i < trajectory.Count; i++)
            {
                _trajectoryArray[i] = trajectory[i];
            }
            _isDataLoaded = true;
            Debug.Log($"[GhostDriver] Loaded trajectory with {trajectory.Count} frames");

            // ИЗМЕНЕНИЕ: Устанавливаем начальную позицию сразу после загрузки
            if (_trajectoryArray.Length > 0)
            {
                var firstFrame = _trajectoryArray[0];
                transform.position = firstFrame.Position;
                transform.rotation = firstFrame.Rotation;
                Debug.Log("[GhostDriver] Set initial position from trajectory");
            }
        }
        else
        {
            _trajectoryArray = null;
            _isDataLoaded = false;
            Debug.LogWarning("[GhostDriver] Empty or null trajectory provided");
        }
    }

    public void Load(VehicleSnapshot[] trajectory)
    {
        _trajectoryArray = trajectory;
        if (trajectory != null && trajectory.Length > 0)
        {
            _isDataLoaded = true;
            Debug.Log($"[GhostDriver] Loaded trajectory array with {trajectory.Length} frames");

            // ИЗМЕНЕНИЕ: Устанавливаем начальную позицию
            var firstFrame = trajectory[0];
            transform.position = firstFrame.Position;
            transform.rotation = firstFrame.Rotation;
            Debug.Log("[GhostDriver] Set initial position from trajectory array");
        }
        else
        {
            _isDataLoaded = false;
            Debug.LogWarning("[GhostDriver] Empty or null trajectory array provided");
        }
    }

    public void SetPlaybackSpeed(float multiplier)
    {
        _speed = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[GhostDriver] Playback speed set to {_speed}");
    }

    public void StartPlayback()
    {
        if (!_isDataLoaded || _trajectoryArray == null || _trajectoryArray.Length == 0)
        {
            Debug.LogWarning("[GhostDriver] No trajectory data available for playback");
            return;
        }

        // ИЗМЕНЕНИЕ: Переключаем в физический режим только при старте воспроизведения
        if (_rb != null)
        {
            _rb.isKinematic = false;
        }

        _startTime = Time.time;
        _index = 0;
        _running = true;

        Debug.Log($"[GhostDriver] Starting playback with {_trajectoryArray.Length} frames at speed {_speed}x");
    }

    public void StopPlayback()
    {
        _running = false;

        // ИЗМЕНЕНИЕ: Возвращаем в кинематический режим при остановке
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[GhostDriver] Playback stopped and ghost frozen");
    }

    private void OnEnable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.AddListener(StartPlayback);
            Debug.Log("[GhostDriver] Subscribed to race started signal");
        }
        else
        {
            Debug.LogError("[GhostDriver] RaceStartedSignal not assigned!");
        }
    }

    private void OnDisable()
    {
        if (_raceStartedSignal != null)
        {
            _raceStartedSignal.RemoveListener(StartPlayback);
            Debug.Log("[GhostDriver] Unsubscribed from race started signal");
        }
    }

    private void FixedUpdate()
    {
        // ИЗМЕНЕНИЕ: Проверяем также флаг загруженности данных
        if (!_running || !_isDataLoaded || _trajectoryArray == null || _trajectoryArray.Length == 0 || _rb == null)
            return;

        float elapsed = (Time.time - _startTime) * _speed;

        // Находим нужный кадр
        while (_index < _trajectoryArray.Length - 1 && _trajectoryArray[_index + 1].Time <= elapsed)
        {
            _index++;
        }

        // Проверяем границы массива
        if (_index >= _trajectoryArray.Length)
        {
            _running = false;
            Debug.Log("[GhostDriver] Reached end of trajectory");
            return;
        }

        var currentFrame = _trajectoryArray[_index];

        // Интерполяция между кадрами для плавности
        if (_index < _trajectoryArray.Length - 1)
        {
            var nextFrame = _trajectoryArray[_index + 1];
            float frameProgress = 0f;

            if (nextFrame.Time > currentFrame.Time)
            {
                frameProgress = (elapsed - currentFrame.Time) / (nextFrame.Time - currentFrame.Time);
                frameProgress = Mathf.Clamp01(frameProgress);
            }

            // Интерполируем позицию и поворот
            _rb.position = Vector3.Lerp(currentFrame.Position, nextFrame.Position, frameProgress);
            _rb.rotation = Quaternion.Lerp(currentFrame.Rotation, nextFrame.Rotation, frameProgress);
            _rb.linearVelocity = Vector3.Lerp(currentFrame.Velocity, nextFrame.Velocity, frameProgress) * _speed;
            _rb.angularVelocity = Vector3.Lerp(currentFrame.AngularVelocity, nextFrame.AngularVelocity, frameProgress) * _speed;
        }
        else
        {
            // Последний кадр - без интерполяции
            _rb.position = currentFrame.Position;
            _rb.rotation = currentFrame.Rotation;
            _rb.linearVelocity = currentFrame.Velocity * _speed;
            _rb.angularVelocity = currentFrame.AngularVelocity * _speed;
        }

        // Проверяем завершение
        if (_index >= _trajectoryArray.Length - 1)
        {
            _running = false;
            Debug.Log("[GhostDriver] Playback completed successfully");
        }
    }
}