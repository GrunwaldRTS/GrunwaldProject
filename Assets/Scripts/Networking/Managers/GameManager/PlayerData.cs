using System;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public int Wood;
    public int Steel;
    public int Food;
    public PlayerData(int wood, int steel, int food)
    {
        Wood = wood;
        Steel = steel;
        Food = food;
    }

    public bool Equals(PlayerData other)
    {
        return Wood == other.Wood && Steel == other.Steel && Food == other.Food;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Wood);
        serializer.SerializeValue(ref Steel);
        serializer.SerializeValue(ref Food);
    }
}