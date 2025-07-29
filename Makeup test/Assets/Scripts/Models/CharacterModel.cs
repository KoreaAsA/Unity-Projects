// Хранит булевы флаги макияжа
public class CharacterModel
{
    public bool HasAcne      { get; private set; } = true;
    public bool HasEyeshadow { get; private set; } = false;
    public bool HasLipstick  { get; private set; } = false;
    public bool HasBlush     { get; private set; } = false;

    public void RemoveAcne()      => HasAcne = false;
    public void ApplyEyeshadow()  => HasEyeshadow = true;
    public void ApplyLipstick()   => HasLipstick = true;
    public void ApplyBlush()      => HasBlush = true;

    public void Reset()
    {
        HasAcne = HasEyeshadow = HasLipstick = HasBlush = true;
    }
}