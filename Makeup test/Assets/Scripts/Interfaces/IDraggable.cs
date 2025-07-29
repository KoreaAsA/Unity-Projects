using UnityEngine;
public interface IDraggable
{
    void OnStartDrag();
    void OnDragging(Vector2 position);
    void OnEndDrag(Vector2 position);
}