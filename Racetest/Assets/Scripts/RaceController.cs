using UnityEngine;

public class RaceController : MonoBehaviour
{
    [Header("Signal Assets - перетащите из Project")]
    [SerializeField] private RaceStartedSignal raceStartedSignal;     // Перетащите .asset файл сюда
    [SerializeField] private RaceFinishedSignal raceFinishedSignal;   // Перетащите .asset файл сюда
    [SerializeField] private CountdownFinishedSignal countdownSignal;  // Перетащите .asset файл сюда

    void Start()
    {
        // Подписываемся на конкретные сигналы
        raceStartedSignal.AddListener(OnRaceStarted, "RaceController");
        raceFinishedSignal.AddListener(OnRaceFinished, "RaceController");
        countdownSignal.AddListener(OnCountdownFinished, "RaceController");
    }

    void OnDestroy()
    {
        // Отписываемся
        raceStartedSignal?.RemoveListener(OnRaceStarted, "RaceController");
        raceFinishedSignal?.RemoveListener(OnRaceFinished, "RaceController");
        countdownSignal?.RemoveListener(OnCountdownFinished, "RaceController");
    }

    // Методы-обработчики
    private void OnRaceStarted()
    {
        Debug.Log("Гонка началась!");
    }

    private void OnRaceFinished(float time)
    {
        Debug.Log($"Гонка закончена за {time} секунд!");
    }

    private void OnCountdownFinished()
    {
        Debug.Log("Обратный отсчет закончен!");
        // Запускаем гонку
        raceStartedSignal.Raise();
    }

    // Методы для вызова сигналов
    [ContextMenu("Start Race")]
    public void StartRace()
    {
        raceStartedSignal.Raise();
    }

    [ContextMenu("Finish Race")]
    public void FinishRace()
    {
        raceFinishedSignal.Raise(120.5f); // Время гонки
    }

    [ContextMenu("Finish Countdown")]
    public void FinishCountdown()
    {
        countdownSignal.Raise();
    }
}