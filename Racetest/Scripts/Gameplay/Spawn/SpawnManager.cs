using UnityEngine;

public sealed class SpawnManager : MonoBehaviour
{
    [Header("Spawns")]
    [SerializeField] private Transform _playerSpawn;
    [SerializeField] private Transform _ghostSpawn;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _ghostPrefab;

    public GameObject SpawnPlayer() => Instantiate(_playerPrefab, _playerSpawn.position, _playerSpawn.rotation);
    public GameObject SpawnGhost()  => Instantiate(_ghostPrefab,  _ghostSpawn.position,  _ghostSpawn.rotation);
}