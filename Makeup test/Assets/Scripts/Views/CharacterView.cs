using UnityEngine;

// Отображает состояние макияжа (включает/выключает спрайты)
public class CharacterView : MonoBehaviour
{
    [Header("Слои макияжа")]
    [SerializeField] private SpriteRenderer acneLayer;
    [SerializeField] private SpriteRenderer eyeshadowLayer;
    [SerializeField] private SpriteRenderer lipstickLayer;
    [SerializeField] private SpriteRenderer blushLayer;

    private CharacterModel _model;

    public void Init(CharacterModel model)
    {
        _model = model;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (acneLayer)      acneLayer.enabled      = _model.HasAcne;
        if (eyeshadowLayer) eyeshadowLayer.enabled = _model.HasEyeshadow;
        if (lipstickLayer)  lipstickLayer.enabled  = _model.HasLipstick;
        if (blushLayer)     blushLayer.enabled     = _model.HasBlush;
    }

    public void ResetVisuals()
    {
        _model.Reset();
        UpdateVisuals();
    }
}