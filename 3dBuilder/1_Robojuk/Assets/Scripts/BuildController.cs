using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


public class BuildController : MonoBehaviour
{
    public GameObject[] blocks;
    public List<Vector3> BlocksCenter = new List<Vector3>();
    public MovementController movementController;
    public List<Sprite> sprites;

    public CameraScript cameraScript;

    public Button[] buttons;
    public Button playBuild;
    public Button stopBuild;
    public Button switchBuild;

    public BuildBar buildBar;
    public InputField inputField;
    public Text placeHolder;
    public Button Exit;

    public Image img;
    public int activeCount = 0;
    public int numberOfBlock;
    public int countOfBlocks;
    public int speed = 2;

    public int inputNumber = 0;

    public bool isGameStarted = false;
    public bool isModelBuilding = false;
    public bool isBlockActivated = false;
    public bool isSwitchPressed = false;
    public bool isStopButtonPressed = false;
    public bool isMoveOnBlock = false;

    public float buildTime = 0f;

    public List<Outline> outlines;

    public int currentIndex = 0;
    public int currentBuildIn = -1;

    public float CameraSpeed;
    public float moveSpeed = 3f;
    public float sensitivity = 3; 

    private Dictionary<int, int[]> _blockIndices = new Dictionary<int, int[]>
    {
        { 0, new [] { 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 } },
        { 3, new [] { 3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 } },
        { 6, new [] { 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 } },
        { 8, new [] { 8, 9, 10, 11, 12, 12, 12, 12, 12, 12, 12, 12 } },
        { 13, new [] { 13, 14, 15, 16, 16, 16, 16, 16, 16, 16, 16, 16 } },
        { 17, new [] { 17, 18, 19, 20, 21, 22, 22, 22, 22, 22, 22, 23 } },
        { 24, new [] { 24, 25, 26, 27, 28, 29, 30, 31, 32, 32, 32, 32 } },
        { 33, new [] { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 42, 42 } },
        { 43, new [] { 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 52, 52 } },
        { 53, new [] { 53, 54, 55, 56, 57, 58, 59, 60, 61, 61, 61, 61 } },
        { 62, new [] { 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 72 } },
        { 73, new [] { 73, 74, 75, 76, 76, 76, 76, 76, 76, 76, 76, 76 } },
        { 77, new [] { 77, 78, 79, 80, 81, 82, 83, 84, 85, 85, 86, 87 } },
        { 88, new [] { 88, 89, 90, 91, 91, 91, 91, 91, 91, 91, 91, 91 } },
        { 90, new [] { 88, 89, 90, 91, 91, 91, 91, 91, 91, 91, 91, 91 } },
    };


    private void Awake()
    {

        isGameStarted = false;
        foreach (var block in blocks)
        {
            countOfBlocks++;
            block.SetActive(false);
        }
        foreach (var block in blocks)
        {
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                Vector3 center = renderer.bounds.center;
                BlocksCenter.Add(center);
                Debug.Log("Центр объекта: " + center);
            }
            else
            {
                Debug.Log("Объект не имеет компонента Renderer.");
            }
        }
    }
    void Start()
    {

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].name = (i + 1).ToString();
        }
        for (int i = 0; i < outlines.Count; i++)
        {
            outlines[i].enabled = false;
        }
        placeHolder.text = $"{currentIndex} / {countOfBlocks}";

    }

    private void Update()
    {
        placeHolder.text = $"{currentIndex} / {countOfBlocks}";


        if (!isModelBuilding)
        {
            isStopButtonPressed = false;
        }

        movementController.currentWaypointIndex = currentIndex;

        currentBuildIn = currentIndex;
        int.TryParse(inputField.text, out inputNumber);
        if (inputNumber < 0 || inputNumber > countOfBlocks)
        {
            inputNumber = 0;
            currentIndex = inputNumber;
        }
        if (inputNumber == 0)
        {
            switchBuild.gameObject.SetActive(false);
        }
        else
        {
            switchBuild.gameObject.SetActive(true);
        }

        if (isModelBuilding)
        {
            foreach (var button in buttons)
            {
                button.interactable = false; // делаем не активной
            }

            playBuild.gameObject.SetActive(false);
            stopBuild.gameObject.SetActive(true);
        }
        else
        {
            foreach (var button in buttons)
            {
                button.interactable = true; // делаем активной
            }

            playBuild.gameObject.SetActive(true);
            stopBuild.gameObject.SetActive(false);
        }

        foreach (var blockIndex in _blockIndices.Keys)
        {
            if (blocks[blockIndex].activeInHierarchy)
            {
                LayerGroupVisibleSetup(blockIndex, _blockIndices[blockIndex]);
            }
        }
    }
    
    /// <summary>
    ///  UI BUTTONS
    /// </summary>
    /// 
    public void SwitchToBlock()
    {
        cameraScript.ObjectIsChoosed = false;
        isSwitchPressed = true;
        foreach (var block in blocks)
        {
            block.SetActive(false);
        }
        for (int i = 0; i < inputNumber; i++)
        {
            blocks[i].SetActive(true);
            currentIndex = inputNumber;
        }
        inputField.text = " ";
        isSwitchPressed = false;
        cameraScript.ObjectIsChoosed = false;

        var indexx = currentIndex - 1;
        if (indexx <= 0)
            indexx = 1;
        if (indexx >= blocks.Length)
            indexx = blocks.Length - 1;
        Camera.main.transform.LookAt(blocks[indexx].transform.position);
    }
    public async void BuildIn()
    {
        var indexx = currentIndex - 1;
        if (indexx <= 0)
            indexx = 1;
        if (indexx >= blocks.Length)
            indexx = blocks.Length - 1;
        inputField.text = " ";
        if (!isStopButtonPressed)
        {
            for (int i = currentIndex; i < blocks.Length && !blocks[i].activeInHierarchy; i++)
            {
                blocks[i].SetActive(true);
                for (int x = 0; x < outlines.Count; x++)
                {
                    outlines[x].enabled = false;
                    if (x == outlines.Count - 1)
                        outlines[currentIndex].enabled = true;
                }
                img.sprite = sprites[i + 1];
                isModelBuilding = true;
                await UniTask.DelayFrame(20);
                currentBuildIn++;
                currentIndex++;
                movementController.MoveToWaypoint(movementController.currentWaypointIndex);
                if (isStopButtonPressed)
                {
                    break;
                }
            }
        }
        isModelBuilding = false;
        
    }
    public void BuildStop()
    {
        cameraScript.ObjectIsChoosed = false;
        isStopButtonPressed = true;
    }


    public void Add()
    {
        cameraScript.ObjectIsChoosed = false;
        inputField.text = " ";
        for (int i = 0; i < blocks.Length; i++)
        {
            var j = blocks[i];
            if (!j.activeInHierarchy)
            {
                j.SetActive(true);
                for (int x = 0; x < outlines.Count; x++)
                {
                    outlines[x].enabled = false;
                    if (x == outlines.Count - 1)
                        outlines[currentIndex].enabled = true;
                }
                img.sprite = sprites[i + 1];
                isBlockActivated = true;
                currentIndex++;
                currentBuildIn++;
                return;
            }
            isBlockActivated = false;
        }
    }

    public void Del()
    {
        cameraScript.ObjectIsChoosed = false;
        inputField.text = " ";
        if (currentIndex >= 0)
        {
            for (int i = blocks.Length - 1; i >= 0; i--)
            {
                var j = blocks[i];
                if (j.activeInHierarchy)
                {
                    outlines[i].enabled = false;
                    if (currentIndex != 1)
                    {
                        outlines[currentIndex - 2].enabled = true;
                    }
                    else if (currentIndex == 1)
                    {
                        outlines[currentIndex - 1].enabled = true;
                    }
                    else
                    {
                        outlines[currentIndex].enabled = true;
                    }
                    img.sprite = sprites[i];
                    j.SetActive(false);
                    isBlockActivated = true;
                    currentIndex--;
                    currentBuildIn--;
                    return;
                }
                isBlockActivated = false;
            }

        }
    }
    public void ExitOnButton()
    {
        Application.Quit();
    }

    /// <summary>
    ///  UI BUTTONS
    /// </summary>
    /// 

    public void LayerGroupVisibleSetup(int blockIndex, int[] layerIndices)
    {
        UnvisibleLayerGroupController();
        VisibleLayerGroupController(blockIndex, layerIndices);
    }

    public void VisibleLayerGroupController(int blockIndex, int[] layerIndices)
    {
        foreach (int layerIndex in layerIndices)
        {
            blocks[layerIndex].layer = LayerMask.NameToLayer("Visible");
        }
    }

    public void UnvisibleLayerGroupController()
    {
        foreach (GameObject obj in blocks)
        {
            obj.layer = LayerMask.NameToLayer("Unvisible");
        }
    }
}
