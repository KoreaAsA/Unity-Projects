using System.Collections.Generic;

public interface IRecorder
{
    void RecordFrame(VehicleSnapshot snapshot);
    IReadOnlyList<VehicleSnapshot> GetTrajectory();
    void Clear();
}