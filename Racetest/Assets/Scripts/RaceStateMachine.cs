using UnityEngine;

public sealed class RaceStateMachine : MonoBehaviour
{
    public static RaceStateMachine Instance { get; private set; }

    public System.Action<RaceState> OnStateChanged;
    public RaceState Current { get; private set; } = RaceState.Idle;

    private void Awake()
    {
        // Singleton pattern с проверкой дубликатов
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[RaceStateMachine] Duplicate instance found! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[RaceStateMachine] Initialized. Current state: {Current}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ChangeState(RaceState newState)
    {
        if (Current == newState)
        {
            Debug.Log($"[RaceStateMachine] State {newState} already active, skipping");
            return;
        }

        var previousState = Current;
        Current = newState;

        Debug.Log($"[RaceStateMachine] State changed: {previousState} → {newState}");
        OnStateChanged?.Invoke(newState);
    }
}