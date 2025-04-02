using UnityEngine;
using UnityEngine.UI;

public class BuildBar : MonoBehaviour
{
    public BuildController buildController;
    public Image progressBar;

    public float CountOfBlocks;
    public float LastBlockRemove;

    void Start()
    {
        CountOfBlocks = buildController.countOfBlocks;

    }

    private void Update()
    {
        LastBlockRemove = buildController.currentIndex;
        progressBar.fillAmount = LastBlockRemove / CountOfBlocks;
    }
}
