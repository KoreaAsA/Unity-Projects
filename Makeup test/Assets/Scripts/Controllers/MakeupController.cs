using UnityEngine;
using System.Collections;

// Управляет логикой макияжа: берёт предмет, двигает руку, применяет эффект
public class MakeupController : MonoBehaviour
{
    [SerializeField] private CharacterView characterView;
    [SerializeField] private HandView handView;
    [SerializeField] private FaceAreaView faceAreaView;

    private CharacterModel _characterModel = new CharacterModel();
    private HandModel _handModel = new HandModel();
    private MakeupItemView _currentItem;
    private Vector3 _originalHandPos;

    private void Awake()
    {
        _originalHandPos = handView.transform.position;
    }

    // Игрок кликнул на предмет: рука летит к нему
    public void PickItem(MakeupItemView item)
    {
        if (_handModel.IsBusy) return;

        _currentItem = item;
        _handModel.PickUp(item.makeupType);

        handView.MoveTo(item.transform.position, () =>
        {
            handView.PlayGrab();          // сжимаем кулак
            item.gameObject.SetActive(false); // прячем предмет
        });
    }

    // Рука летит за курсором (пока зажата ЛКМ)
    public void DragTo(Vector2 pointerPos)
    {
        if (!_handModel.IsBusy) return;
        handView.MoveTo(pointerPos);
    }

    // Игрок отпустил кнопку
    public void EndDrag(Vector2 pointerPos)
    {
        if (!_handModel.IsBusy) return;

        if (faceAreaView.IsInsideFace(pointerPos))
        {
            // летим к лицу и проигрываем нанесение
            handView.MoveTo(pointerPos, () =>
            {
                handView.PlayApply();
                ApplyEffect();
            });
        }
        else
        {
            // возвращаем предмет и руку
            handView.MoveTo(_currentItem.transform.position, () =>
            {
                _currentItem.gameObject.SetActive(true);
                handView.MoveTo(_originalHandPos, null);
            });
        }

        _handModel.Drop();
        _currentItem = null;
    }

    // Применяем эффект в зависимости от предмета
    private void ApplyEffect()
    {
        switch (_handModel.CurrentItem)
        {
            case MakeupType.Cream: _characterModel.RemoveAcne(); break;
            case MakeupType.Eyeshadow: _characterModel.ApplyEyeshadow(); break;
            case MakeupType.Lipstick: _characterModel.ApplyLipstick(); break;
            case MakeupType.Blush: _characterModel.ApplyBlush(); break;
        }
        characterView.UpdateVisuals();
    }

    // Спонж стирает макияж
    public void ResetAll()
    {
        _characterModel.Reset();
        characterView.ResetVisuals();
    }
}