using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MovementController : MonoBehaviour
{
    public CameraScript cameraScript;
    public BuildController buildController;
    public Vector3[] coordinatesData;
    public int currentWaypointIndex = 0;
    public int currentRotationIndex = 0;
    public float speed = 0.1f;
    public Vector3 lastPosition;
    public bool isMoving;
    public Vector3 targetPosition;
    public Vector3 lookDirection;
    public int indexx;
    public Vector3 newCameraPosition;
    public Vector3 directionToObject;
    public Quaternion currentRotation;
    public Vector3 startLastPosition;
    public Vector3 currentPosition;

    private void Awake()
    {
        coordinatesData = new Vector3[]
{
         new Vector3(7.1f, 75.6f, -320f),//0
         new Vector3(7.1f, 55.6f, -370f),//1
         new Vector3(7.1f, 75.6f, -320f),//2
         new Vector3(120f, 62.8f, -279f),//3
         new Vector3(320f, 62.8f, -279f),//4
         new Vector3(320f, 62.8f, -279f),//5
         new Vector3(320f, 62.8f, -279f),//6
         new Vector3(122.2f, 28.4f, -253.5f),//7
         new Vector3(120.4f, 63f, 200f),//8
         new Vector3(100f, 47.3f, -222.7f),//9
         new Vector3(70f, 47.3f, -288.7f),//10
         new Vector3(-33f, 47.3f, -320.7f),//11
         new Vector3(-8.5f, 130f, -320.7f),//12
         new Vector3(0f, 139f, -300f),//13
         new Vector3(100f, 50f, -300f),//14
         new Vector3(100f, 50f, -300f),//15
         new Vector3(100.6f, 145.7f, -265.9f),//16
         new Vector3(250.6f, 45.7f, -365.9f),//17
         new Vector3(250.6f, 45.7f, -365.9f),//18
         new Vector3(250.6f, 145.7f, -365.9f),//19
         new Vector3(150.6f, 145.7f, -365.9f),//20
         new Vector3(180.6f, 45.7f, -255.9f),//21
         new Vector3(-8.5f, 130f, -320.7f),//22
         new Vector3(-30f, 62f, -350f),//23
         new Vector3(7.1f, 75.6f, -320f),//24
         new Vector3(-24f, 106f, -352f),//25
         new Vector3(-24f, 106f, -352f),//26
         new Vector3(-24f, 106f, -352f),//27
         new Vector3(-79.2f, 93f, -273f),//28
         new Vector3(-79.2f, 93f, -273f),//29
         new Vector3(-79.2f, 93f, -273f),//30
         new Vector3(-79.2f, 93f, -273f),//31
         new Vector3(-34.2f, 60.6f, -373f),//32
         new Vector3(-34.2f, 60.6f, -373f),//33
         new Vector3(-34.2f, 60.6f, -373f),//34
         new Vector3(-31.2f, 113f, -431.3f),//35
         new Vector3(-70.2f, 53f, -301.3f),//36
         new Vector3(-70.2f, 53f, -301.3f),//37
         new Vector3(60f, 77f, -370f),//38
         new Vector3(118f, 46f, -310f),//39
         new Vector3(52f, 54f, -376f),//40
         new Vector3(-70.2f, 53f, -371.3f),//41
         new Vector3(152f, 54f, -376f),//42
         new Vector3(0f, 50f, 320f),//43
         new Vector3(0f, 50f, 460f),//44
         new Vector3(0f, 50f, 320f),//45
         new Vector3(232f, 50f, 150f),//46
         new Vector3(-40f, 50f, 420f),//47
         new Vector3(232f, 30f, -40f),//48
         new Vector3(172f, 30f, -130f),//49
         new Vector3(252f, 90f, 320f),
         new Vector3(252f, 90f, 320f),//51
         new Vector3(0f, 50f, 320f),//52
         new Vector3(0f, 50f, 320f),//53
         new Vector3(0f, 50f, 320f),//54
         new Vector3(0f, 50f, 320f),//55
         new Vector3(0f, 50f, 320f),//56
         new Vector3(0f, 50f, 320f),//57
         new Vector3(0f, 50f, 320f),//58
         new Vector3(0f, 50f, 320f),//59
         new Vector3(0f, 50f, 320f),//60
         new Vector3(0f, 50f, 320f),//61
         new Vector3(0f, 50f, 320f),//62
         new Vector3(0f, 50f, 320f),//63
         new Vector3(0f, 50f, 320f),//64
         new Vector3(0f, 50f, 320f),//65
         new Vector3(0f, 50f, 320f),//66
         new Vector3(0f, 50f, 320f),//67
         new Vector3(0f, 50f, 320f),//68
         new Vector3(0f, 50f, 320f),//69
         new Vector3(0f, 50f, 320f),//70
         new Vector3(0f, 50f, 320f),//71
         new Vector3(0f, 50f, 320f),//72
         new Vector3(32.5f, 111f, -325.3f),//73
         new Vector3(32.5f, 111f, -325.3f),//74
         new Vector3(-152.5f, 131f, -320.5f),//75
         new Vector3(-152.5f, 131f, -320.5f),//76
         new Vector3(257.4f, 5.3f, -117.1f),//77
         new Vector3(257.4f, 5.3f, -117.1f),//78
         new Vector3(257.4f, 5.3f, -117.1f),//79
         new Vector3(257.4f, 5.3f, -117.1f),//80
         new Vector3(257.4f, 5.3f, -117.1f),//81
         new Vector3(257.4f, 5.3f, -117.1f),//82
         new Vector3(257.4f, 5.3f, -117.1f),//83
         new Vector3(257.4f, 5.3f, -117.1f),//84
         new Vector3(257.4f, 5.3f, -117.1f),//85
         new Vector3(257.4f, 5.3f, -117.1f),//86
         new Vector3(257.4f, 5.3f, -117.1f),//87
         new Vector3(257.4f, 5.3f, -117.1f),//88
         new Vector3(257.4f, 5.3f, -117.1f),//89
         new Vector3(357.4f, 90.3f, -117.1f),//90
         new Vector3(-147.4f, 90.3f, -310.1f),//91
         new Vector3(257.4f, 5.3f, -117.1f),//92
         new Vector3(257.4f, 5.3f, -117.1f),//93
         new Vector3(357.4f, 90.3f, -117.1f),//94
         new Vector3(-147.4f, 90.3f, -310.1f),//95
        };

    }

    void Update()
    {
        currentRotation = transform.rotation;
        currentPosition = transform.position;

        if (Vector3.Distance(currentPosition, lastPosition) > 1f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        indexx = buildController.currentIndex - 1;
        if (indexx <= 0)
            indexx = 1;
        if (indexx >= buildController.blocks.Length)
            indexx = buildController.blocks.Length - 1;
        targetPosition = buildController.BlocksCenter[indexx];
    }

    public void MoveToWaypoint(int index)
    {
        if (index < 0 || index >= coordinatesData.Length)
        {
            Debug.LogWarning("Invalid waypoint index");
            return;
        }

        StartCoroutine(MoveBetweenWaypoints(index));
        Debug.Log("Moving to waypoint " + coordinatesData[index] + index);
    }


    private Dictionary<int, Vector3> specificRotationsEuler = new Dictionary<int, Vector3>
    {
    };

    IEnumerator MoveBetweenWaypoints(int index)
    {
        isMoving = true;

        Vector3 targetPosition = coordinatesData[index];
        startLastPosition = transform.position;
        float deltaTimeSpeed = Time.deltaTime * speed;
        float t = 0f;

        while (t < 1f)
        {
            t += deltaTimeSpeed;
            transform.position = Vector3.Lerp(startLastPosition, targetPosition, t);

            var indexx = buildController.currentIndex - 1;
            if (indexx <= 0)
                indexx = 1;
            if (indexx >= buildController.blocks.Length)
                indexx = buildController.blocks.Length - 1;

            directionToObject = (buildController.blocks[indexx].transform.position - transform.position).normalized;
            newCameraPosition = buildController.blocks[indexx].transform.position - directionToObject * Camera.main.nearClipPlane * 900f;

            transform.position = newCameraPosition;
            transform.LookAt(buildController.BlocksCenter[indexx]);

            yield return null;
        }

        cameraScript.UpdateOffsetAndPan(buildController.BlocksCenter[indexx]);

        isMoving = false;
    }


    public void OnForwardButtonPressed()
    {
        if (currentWaypointIndex < coordinatesData.Length - 1)
        {
            currentWaypointIndex++;
            MoveToWaypoint(currentWaypointIndex);
        }
    }

    public void OnBackwardButtonPressed()
    {
        if (currentWaypointIndex > 0)
        {
            currentWaypointIndex--;
            MoveToWaypoint(currentWaypointIndex);
        }
    }
}
