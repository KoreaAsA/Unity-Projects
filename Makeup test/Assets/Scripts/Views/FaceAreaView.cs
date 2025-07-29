using UnityEngine;

public class FaceAreaView : MonoBehaviour
{
    [SerializeField] private Collider2D faceCollider;

    public bool IsInsideFace(Vector2 position)
    {
        return faceCollider.OverlapPoint(position);
    }
}