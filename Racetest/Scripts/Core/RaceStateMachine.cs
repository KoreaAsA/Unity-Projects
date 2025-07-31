using UnityEngine;

public enum RaceState { Idle, Countdown, Racing, Finished, Paused }

public sealed class RaceStateMachine : MonoBehaviour
{
    public static RaceStateMachine Instance { get; private set; }

    public System.Action<RaceState> OnStateChanged;

    [field: SerializeField] public RaceState Current { get; private set; } = RaceState.Idle;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogError("[RaceStateMachine] Попытка второго инстанса!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(RaceState newState)
    {
        if (Current == newState) return;
        Current = newState;
        Debug.Log($"[RaceStateMachine] Состояние: {newState}");
        OnStateChanged?.Invoke(newState);
    }
}