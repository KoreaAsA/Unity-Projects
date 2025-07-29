using UnityEngine;
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private MakeupController makeupController;
    [SerializeField] private MakeupItemView[] makeupItems;

    void Start()
    {
        foreach (var item in makeupItems)
        {
            item.Init(makeupController);
        }
    }
}  