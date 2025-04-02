using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CameraScript : MonoBehaviour
{


    [Header("Other scripts")]
    [Space(10)]
    [SerializeField] private BuildController buildController;
    [SerializeField] private MovementController movementController;
    [Header("Objects")]
    [Space(10)]
    [SerializeField] private Camera cam;
    [Header("Zoom")]
    [Space(10)]
    [SerializeField] private float zoomMinFov = 20f;
    [SerializeField] private float zoomMaxFov = 150;
    [SerializeField] private float zoomMinDistance = 2f;
    [SerializeField] private float zoomMaxDistance = 25;
    [SerializeField] private float zoomInDistance = 2f;

    [SerializeField] private Transform target;
    [SerializeField] private int indexx;
    private int previousIndex;
    [SerializeField] public float offsetTouchZoom;
    [SerializeField] private Vector3 offsetVector;

    [Header("Booleans trues or false")]
    [Space(10)]
    [SerializeField]
    private bool objectIsChoosed;


    public bool ObjectIsChoosed
    {
        set { objectIsChoosed = value; }
        get { return objectIsChoosed; }
    }

    private bool IsTouchDevice;
    private bool IsRotating;
    private bool IsZooming;
    private bool IsTouchHandle;
    private bool IsMouseHandle;
    private bool IsMobileBrowser;


    [Header("Mouse sensitivity")]
    [Space(10)]
    [SerializeField] private float sensitivity = 5;
    [SerializeField] private float touchSensitivity = .25f;

    [Header("Limits")]
    [Space(10)]
    [SerializeField] private float limit = 80;
    [SerializeField] private float X, Y;

    [SerializeField] private Vector3 lastPanPosition;
    [SerializeField] private int panFingerId;
    [SerializeField] private bool wasZoomingLastFrame;
    [SerializeField] private Vector2[] lastZoomPositions;
    [SerializeField] private float zoomSpeedTouch = 0.1f;
    [SerializeField] private float rotationSpeed = 25f;


    void Awake()
    {
        cam = GetComponent<Camera>();
    }
    void Start()
    {


        Debug.Log(SystemInfo.deviceType);
        Input.multiTouchEnabled = true;
        IsTouchDevice = SystemInfo.deviceType == DeviceType.Handheld;

#if UNITY_WEBGL
        IsMobileBrowser = Application.platform == RuntimePlatform.WebGLPlayer && _checkMobileBrowser() == 1;
#else
        IsMobileBrowser = false;
#endif
        if (IsMobileBrowser)
        {
            Debug.Log("Running in a mobile browser");
        }
        else
        {
            Debug.Log("Running in a non-mobile browser or non-WebGL platform");
        }
        previousIndex = indexx;
    }

    [DllImport("__Internal")]
    private static extern int _checkMobileBrowser();
    void Update()
    {
        indexx = buildController.currentIndex;
        if (indexx <= 0)
            indexx = 1;
        else if (indexx >= buildController.blocks.Length)
            indexx = buildController.blocks.Length - 1;

        if (previousIndex != indexx)
        {
            previousIndex = indexx;
            target = buildController.blocks[indexx - 1].transform;
        }

        HandleRayCast();

        if (Input.GetMouseButtonUp(0) && ObjectIsChoosed)
        {
            ObjectIsChoosed = false;
        }

        if (!Input.GetMouseButton(0) && ObjectIsChoosed)
        {
            ObjectIsChoosed = false;
        }


        if (IsMobileBrowser && ObjectIsChoosed)
        {
            HandleTouch();
        }

        if (!IsMobileBrowser)
        {
            if (Input.GetMouseButton(0) && ObjectIsChoosed)
            {
                HandleMouseInput();
            }
        }

    }
    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1:
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    RotateCameraTouch(touch.position);
                }
                break;

            case 2:
                if (Input.GetTouch(0).phase == TouchPhase.Ended ||
                    Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    var t = Input.touches.First(touch => touch.phase != TouchPhase.Ended);
                    panFingerId = t.fingerId;
                    lastPanPosition = t.position;
                    return;
                }

                Vector2[] newPositions = { Input.GetTouch(0).position, Input.GetTouch(1).position };

                if (Input.GetTouch(1).phase == TouchPhase.Began ||
                    Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lastZoomPositions = newPositions;
                    return;
                }

                var oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                var newDistance = Vector2.Distance(newPositions[0], newPositions[1]);

                var pan = (newDistance - oldDistance) * Time.deltaTime;

                ZoomCameraTouch(pan, zoomSpeedTouch);

                lastZoomPositions = newPositions;
                break;

            default:
                break;
        }
    }

    void RotateCameraTouch(Vector3 newPanPosition)
    {
        var axis = (newPanPosition - lastPanPosition) * (Time.deltaTime * touchSensitivity);

        offsetVector.z = Mathf.Clamp(offsetVector.z, -Mathf.Abs(zoomMaxDistance), -Mathf.Abs(zoomMinDistance));

        X = transform.localEulerAngles.y + axis.x;
        Y = transform.localEulerAngles.x - axis.y;

        transform.localEulerAngles = new Vector3(Y, X, 0);
        ClampRotation();
        transform.position = targetPos + transform.localRotation * offsetVector;
        lastPanPosition = newPanPosition;
    }

    void ZoomCameraTouch(float pan, float speed)
    {
        if (pan == 0) return;

        offsetVector.z += pan * speed;
        offsetVector.z = Mathf.Clamp(offsetVector.z, -Mathf.Abs(zoomMaxDistance), -Mathf.Abs(zoomMinDistance));

        transform.position = targetPos + (transform.localRotation * offsetVector);
    }

    private void HandleMouseInput()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            var target = targetPos;

            var zoomAmount = 0f;
            if (Input.GetAxis("Mouse ScrollWheel") > 0) zoomAmount += zoomInDistance;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0) zoomAmount -= zoomInDistance;

            if (Input.GetKey(KeyCode.UpArrow)) zoomAmount += zoomInDistance;
            else if (Input.GetKey(KeyCode.DownArrow)) zoomAmount -= zoomInDistance;

            offsetVector.z += zoomAmount;
            offsetVector.z = Mathf.Clamp(offsetVector.z, -Mathf.Abs(zoomMaxDistance), -Mathf.Abs(zoomMinDistance));

            X = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
            Y = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensitivity;

            transform.localEulerAngles = new Vector3(Y, X, 0);
            ClampRotation();
            transform.position = targetPos + (transform.localRotation * offsetVector);
        }
    }

    private void ClampRotation()
    {
        transform.rotation = Clamp(transform.rotation, limit);
    }

    public static Quaternion Clamp(Quaternion rotation, float range)
    {
        rotation.x /= rotation.w;
        rotation.y /= rotation.w;
        rotation.z /= rotation.w;
        rotation.w = 1.0f;

        var pitch = 2.0f * Mathf.Rad2Deg * Mathf.Atan(rotation.x);

        pitch = Mathf.Clamp(pitch, -range, range);
        rotation.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * pitch);

        return rotation;
    }

    private Vector3 targetPos;

    public void UpdateOffsetAndPan(Vector3 targetPosition)
    {
        targetPos = targetPosition;
        lastPanPosition = cam.WorldToScreenPoint(targetPos);
        offsetVector = -cam.ScreenToViewportPoint(lastPanPosition);

        ClampRotation();
        transform.position = (transform.localRotation * offsetVector) + targetPos;
    }

    private void HandleRayCast()
    {
        int layerMask = (1 << 6);
        Ray ray;
        RaycastHit hit;

        if (IsTouchDevice)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    target = hit.transform;
                    ObjectIsChoosed = true;
                    if (hit.collider.gameObject == gameObject)
                    {
                        ObjectIsChoosed = false;
                    }
                }
            }
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                target = hit.transform;
                ObjectIsChoosed = true;
                if (hit.collider.gameObject == gameObject)
                {
                    ObjectIsChoosed = false;
                }
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        break;
                    case TouchPhase.Moved:
                        break;
                    case TouchPhase.Ended:
                        ObjectIsChoosed = false;
                        break;
                }
            }
        }

    }
    void Zoom(float increment)
    {
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - increment, zoomMinFov, zoomMaxFov);
    }

}