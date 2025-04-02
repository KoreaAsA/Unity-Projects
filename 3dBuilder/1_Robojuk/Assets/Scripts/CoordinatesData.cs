using UnityEngine.EventSystems;
using UnityEngine;

public class CameraScript2 : MonoBehaviour
{
    public BuildController buildController;
    public MovementController movementController;
    public float rotationSpeed = 1f;
    public float minZoom = 5f;
    public float maxZoom = 80f;
    public float sensitivity = 1f;
    public float limit = 80f;
    public float zoomIn = 1f;
    public float zoomMin = 5f;
    public float zoomMax = 80f;

    public Camera cam;
    private Vector3 lastPanPosition;
    private Vector3 offset;
    private bool isTouchDevice;
    public Transform target;
    public bool ObjectIsChoosed;
    private int indexx;
    private float X;
    private float Y;

    void Start()
    {
        cam = Camera.main;
        isTouchDevice = Input.touchSupported;
        offset = transform.position - buildController.BlocksCenter[indexx];
    }

    void Update()
    {
        HandleMouseInput();
        HandleRayCast();
    }

    void RotateCamera(Vector3 newPanPosition)
    {
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * rotationSpeed, 0, offset.y * rotationSpeed);

        transform.RotateAround(buildController.BlocksCenter[indexx], Vector3.up, move.x);
        transform.RotateAround(buildController.BlocksCenter[indexx], transform.right, move.z);

        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0) return;

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), minZoom, maxZoom);
    }

    private void HandleMouseInput()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0) offset.z += zoomIn;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0) offset.z -= zoomIn;
            offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));

            X = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
            Y += Input.GetAxis("Mouse Y") * sensitivity;
            Y = Mathf.Clamp(Y, -limit, limit);
            transform.localEulerAngles = new Vector3(-Y, X, 0);
            transform.position = transform.localRotation * offset + buildController.BlocksCenter[indexx];
        }
    }

    private void HandleRayCast()
    {
        int layerMask = (1 << 6);
        Ray ray;
        RaycastHit hit;

        if (isTouchDevice)
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
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - increment, zoomMin, zoomMax);
    }
}