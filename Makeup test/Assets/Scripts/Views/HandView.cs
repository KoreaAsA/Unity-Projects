using UnityEngine;
using System.Collections;

// Отвечает за плавное движение руки и проигрывание простых анимаций
public class HandView : MonoBehaviour
{
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Transform handTransform;
    [SerializeField] private float moveSpeed = 5f;

    private bool isMoving;

    // Плавно летим к точке и вызываем callback по прибытии
    public void MoveTo(Vector3 target, System.Action onArrive = null)
    {
        if (isMoving) return; // защита от повторного вызова
        StartCoroutine(MoveRoutine(target, onArrive));
    }

    private IEnumerator MoveRoutine(Vector3 target, System.Action onArrive)
    {
        isMoving = true;
        while (Vector3.Distance(handTransform.position, target) > 0.05f)
        {
            handTransform.position = Vector3.MoveTowards(
                handTransform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        handTransform.position = target;
        isMoving = false;
        onArrive?.Invoke();
    }

    // Анимации: сжатие кулака и нанесение
    public void PlayGrab() => handAnimator?.Play("Hand_Grab");
    public void PlayApply() => handAnimator?.Play("Hand_Apply");
}