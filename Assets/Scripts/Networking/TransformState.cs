using Unity.Netcode;
using UnityEngine;
public struct TransformState : INetworkSerializable
{
    public int Tick;
    public Vector3 Position;
    public Quaternion Rotation;

    public TransformState(int tick, Vector3 position, Quaternion rotation)
    {
        Tick = tick;
        Position = position;
        Rotation = rotation;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
    }
}
