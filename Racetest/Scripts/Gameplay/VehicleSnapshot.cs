using UnityEngine;

public readonly struct VehicleSnapshot
{
    public readonly float Time;
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Velocity;
    public readonly Vector3 AngularVelocity;

    public VehicleSnapshot(float t, Vector3 p, Quaternion r, Vector3 v, Vector3 av)
    {
        Time = t;
        Position = p;
        Rotation = r;
        Velocity = v;
        AngularVelocity = av;
    }
}