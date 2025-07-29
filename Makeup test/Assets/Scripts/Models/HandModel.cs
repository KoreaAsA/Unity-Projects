using UnityEngine;

public class HandModel
{
    public bool IsBusy { get; private set; } = false;
    public MakeupType? CurrentItem { get; private set; } = null;

    public void PickUp(MakeupType type)
    {
        IsBusy = true;
        CurrentItem = type;
    }

    public void Drop()
    {
        IsBusy = false;
        CurrentItem = null;
    }
}