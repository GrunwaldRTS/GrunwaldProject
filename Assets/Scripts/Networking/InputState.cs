using System;
using UnityEngine;
using Unity.Netcode;

public struct InputState : INetworkSerializable
{
    public int Tick;
    public DateTime TimeStamp;
    public Vector3 NewDestination;
    public Quaternion TargetRotation;
    public InputState(int tick, DateTime timestamp, Vector3 newDestination, Quaternion targetRotation)
    {
        Tick = tick;
        TimeStamp = timestamp;
        NewDestination = newDestination;
        TargetRotation = targetRotation;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref TimeStamp);
        serializer.SerializeValue(ref NewDestination);
        serializer.SerializeValue(ref TargetRotation);
    }
    //public override string ToString()
    //{
    //    return $"Movement: {Movement}, mouse: {MouseDelta}";
    //}
}