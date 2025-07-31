public interface IRecorder
{
    void RecordFrame(VehicleSnapshot snapshot);
    System.Collections.Generic.IReadOnlyList<VehicleSnapshot> GetTrajectory();
    void Clear();
}