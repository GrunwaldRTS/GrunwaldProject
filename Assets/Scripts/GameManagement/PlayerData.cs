using System;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable
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
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Wood);
        serializer.SerializeValue(ref Steel);
        serializer.SerializeValue(ref Food);
    }
}