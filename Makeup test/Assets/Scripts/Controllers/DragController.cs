using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [SerializeField] private InputActionReference dragStart;
    [SerializeField] private InputActionReference drag;
    [SerializeField] private InputActionReference dragEnd;
    [SerializeField] private Camera cam;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        dragStart.action.performed += OnDragStart;
        drag.action.performed += OnDrag;
        dragEnd.action.performed += OnDragEnd;
    }

    private void OnEnable()
    {
        dragStart.action.Enable();
        drag.action.Enable();
        dragEnd.action.Enable();
    }

    private void OnDisable()
    {
        dragStart.action.Disable();
        drag.action.Disable();
        dragEnd.action.Disable();
    }

    // 1) нажали левую кнопку
    private void OnDragStart(InputAction.CallbackContext ctx)
    {
        var pos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var hit = Physics2D.OverlapPoint(pos);
        hit?.GetComponent<IDraggable>()?.OnStartDrag();
    }

    // 2) держим / двигаем
    private void OnDrag(InputAction.CallbackContext ctx)
    {
        var pos = cam.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        // передаём точку всем, кто сейчас «держится»
        foreach (var d in FindObjectsOfType<MonoBehaviour>())
            if (d is IDraggable draggable && d.gameObject.activeInHierarchy)
                draggable.OnDragging(pos);
    }

    // 3) отпустили кнопку
    private void OnDragEnd(InputAction.CallbackContext ctx)
    {
        var pos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        foreach (var d in FindObjectsOfType<MonoBehaviour>())
            if (d is IDraggable draggable && d.gameObject.activeInHierarchy)
                draggable.OnEndDrag(pos);
    }
}