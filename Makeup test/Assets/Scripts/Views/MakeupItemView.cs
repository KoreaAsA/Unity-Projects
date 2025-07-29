using UnityEngine;

// Предмет макияжа: крем, тени, помада, румяна, спонж
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class MakeupItemView : MonoBehaviour, IDraggable
{
    public MakeupType makeupType;

    private MakeupController _controller;
    private Vector3 _originalPos;

    public void Init(MakeupController controller)
    {
        _controller = controller;
        _originalPos = transform.position;
    }

    public Vector3 OriginalPosition => _originalPos;

    public void OnStartDrag() =>
        _controller.PickItem(this);

    public void OnDragging(Vector2 pointerPos) =>
        _controller.DragTo(pointerPos);

    public void OnEndDrag(Vector2 pointerPos) =>
        _controller.EndDrag(pointerPos);
}