using UnityEngine;

public sealed class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _playerSpawn;
    [SerializeField] private Transform _ghostSpawn;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _ghostPrefab;

    // НОВЫЙ МЕТОД: Спавн игрока с учетом логики первого заезда
    public GameObject SpawnPlayer(bool isFirstRace)
    {
        Transform spawnPoint = isFirstRace ? _ghostSpawn : _playerSpawn;
        
        Debug.Log($"[SpawnManager] Spawning player at {(isFirstRace ? "ghost" : "player")} spawn (first race: {isFirstRace})");
        
        return Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    // Оригинальный метод для обратной совместимости
    public GameObject SpawnPlayer() => SpawnPlayer(false);

    public GameObject SpawnGhost() => Instantiate(_ghostPrefab, _ghostSpawn.position, _ghostSpawn.rotation);
}