using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Target to lock the camera on")]
    private Transform _target;

    [Header("Stats")]
    [Header("Orbiting")]
    [SerializeField]
    private float mouseSensitivity = 3f;
    [SerializeField]
    private float distanceFromTarget = 10f;
    [SerializeField]
    private float OrbitingSmoothTime = 0.2f;
    [SerializeField]
    private Vector2 rotationXMinMax = new(-45, 45);
    [SerializeField]
    private Vector2 rotationYMinMax = new(-99999, 99999);
    [SerializeField]
    private bool invertXAxis = true;
    [Header("Zooming")]
    [SerializeField]
    private bool ActivateZooming = true;
    [SerializeField]
    float MouseZoomSpeed = 15f;
    [SerializeField]
    float TouchZoomSpeed = 0.1f;
    [SerializeField]
    private Vector2 DistanceMinMax = new Vector2(1, 20);

    private CursorLockMode CursorMode;
    private Vector3 currentRotation;
    private Vector3 smoothingVelocity = Vector3.zero;
    private float InputCooldown = 0f;
    private float rotationY;
    private float rotationX;

    private void OnEnable()
    {
        rotationY = this.transform.localEulerAngles.y;
        rotationX = this.transform.localEulerAngles.x;
        currentRotation = this.transform.localEulerAngles;
        CursorMode = CursorLockMode.Locked;
    }

    public void HandleTouchInput()
    {
        if (_target == null)
            return;

        SetCursorState();

        if (Cursor.visible)
            return;

        // Zooming

        if (ActivateZooming)
        {
            if (Input.touchSupported)
            {
                // Pinch zoom
                if (Input.touchCount == 2)
                {
                    InputCooldown = Time.time;
                    // Get touch points
                    Touch Point1 = Input.GetTouch(0);
                    Touch Point2 = Input.GetTouch(1);
                    // get touch points in previous frame
                    Vector2 Point1Previous = Point1.position - Point1.deltaPosition;
                    Vector2 Point2Previous = Point2.position - Point2.deltaPosition;
                    float prevTouchDistance = Vector2.Distance(Point1Previous, Point2Previous);
                    float currentTouchDistance = Vector2.Distance(Point1.position, Point2.position);
                    // Measuring distance change
                    float Distance = prevTouchDistance - currentTouchDistance;
                    Zoom(Distance, TouchZoomSpeed);
                }
            }
            else
            {
                // Zoom using mouse scrollwheel
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                Zoom(scroll, MouseZoomSpeed);
            }
        }

        // Orbiting

        // Halts orbiting input feed until cooldown to prevent Input overlapping 
        if (InputCooldown == 0 || Time.time - InputCooldown > 0.5f)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            if (invertXAxis)
                rotationX -= mouseY;
            else rotationX += mouseY;
            rotationY += mouseX;

            // Apply clamping for vertical rotation 
            rotationX = Mathf.Clamp(rotationX, rotationXMinMax.x, rotationXMinMax.y);
            // Apply clamping for horizontal rotation 
            rotationY = Mathf.Clamp(rotationY, rotationYMinMax.x, rotationYMinMax.y);
            
            Vector3 nextRotation = new Vector3(rotationX, rotationY);

            // Apply damping between rotation changes
            currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothingVelocity, OrbitingSmoothTime);
            transform.localEulerAngles = currentRotation;

        }

        // Pointing the Camera to the target
        transform.position = _target.position - transform.forward * distanceFromTarget;

    }


    private void SetCursorState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorMode = CursorLockMode.None;

        if (Input.GetMouseButtonDown(0))
            CursorMode = CursorLockMode.Locked;

        // Apply cursor state
        Cursor.lockState = CursorMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != CursorMode);
    }

    void Zoom(float deltaMagnitudeDiff, float speed)
    {
        distanceFromTarget = Mathf.Clamp(distanceFromTarget + deltaMagnitudeDiff * speed, DistanceMinMax.x, DistanceMinMax.y);
    }

}