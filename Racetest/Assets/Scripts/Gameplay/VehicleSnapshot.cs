using UnityEngine;

[System.Serializable]
public struct VehicleSnapshot
{
    public float Time;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;

    public VehicleSnapshot(float t, Vector3 p, Quaternion r, Vector3 v, Vector3 av)
    {
        Time = t; Position = p; Rotation = r; Velocity = v; AngularVelocity = av;
    }
}