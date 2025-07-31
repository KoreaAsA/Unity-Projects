public interface IPlayback
{
    void Load(System.Collections.Generic.IReadOnlyList<VehicleSnapshot> trajectory);
    void SetPlaybackSpeed(float multiplier);
    void StartPlayback();
    void StopPlayback();
}