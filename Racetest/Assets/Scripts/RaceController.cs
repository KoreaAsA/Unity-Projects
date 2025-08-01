using UnityEngine;

public class RaceController : MonoBehaviour
{
    [SerializeField] private RaceStartedSignal raceStartedSignal;     
    [SerializeField] private RaceFinishedSignal raceFinishedSignal;   
    [SerializeField] private CountdownFinishedSignal countdownSignal;  

    void Start()
    {
        // Подписываемся
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