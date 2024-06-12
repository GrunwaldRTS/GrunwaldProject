using System;
using UnityEngine;
using Unity.Netcode;

public struct InputState : INetworkSerializable
{
    public int Tick;
    public double Time;
    public Vector3 NewDestination;
    public Quaternion TargetRotation;
    public InputState(int tick, double time, Vector3 newDestination, Quaternion targetRotation)
    {
        Tick = tick;
        Time = time;
        NewDestination = newDestination;
        TargetRotation = targetRotation;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Time);
        serializer.SerializeValue(ref NewDestination);
        serializer.SerializeValue(ref TargetRotation);
    }
    //public override string ToString()
    //{
    //    return $"Movement: {Movement}, mouse: {MouseDelta}";
    //}
}