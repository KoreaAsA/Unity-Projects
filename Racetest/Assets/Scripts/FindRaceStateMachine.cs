using UnityEngine;

public class FindRaceStateMachine : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log($"--- Scene objects with RaceStateMachine ---");
        var all = FindObjectsOfType<RaceStateMachine>();
        foreach (var r in all)
        {
          //Debug.Log($"Found: {r.name} (active={r.gameObject.activeInHierarchy})");
        }

        if (all.Length == 0)
        {
           // Debug.LogError(" Ни одного RaceStateMachine в сцене!");
        }
    }
}