using UnityEngine;
using FishNet.Object.Prediction;

// What the client sends to the server
public struct MoveData : IReplicateData
{
    public Vector3 MoveDirection;

    // Required by IReplicateData
    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

// What the server sends to the client
public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public Quaternion Rotation;

    // Required by IReconcileData
    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}