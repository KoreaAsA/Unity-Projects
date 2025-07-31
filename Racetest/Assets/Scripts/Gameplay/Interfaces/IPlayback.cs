using System.Collections.Generic;

public interface IPlayback
{
    void Load(IReadOnlyList<VehicleSnapshot> trajectory);
    void SetPlaybackSpeed(float multiplier);
    void StartPlayback();
    void StopPlayback();
}
