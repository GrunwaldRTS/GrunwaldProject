using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public struct TransformState : INetworkSerializable
{
    public int Tick;
    public double Time;
    public bool SynchPosX;
    public bool SynchPosY;
    public bool SynchPosZ;
    public float PosX;
    public float PosY;
    public float PosZ;
    public bool SynchRotX;
    public bool SynchRotY;
    public bool SynchRotZ;
    public float RotX;
    public float RotY;
    public float RotZ;

    public TransformState defaultValue => new TransformState(0, 0, Vector3.zero, Vector3.zero, Vector3Int.zero, Vector3Int.zero);
    public TransformState(int tick, double time, Vector3 position, Vector3 rotation) : this(tick, time, position, rotation, Vector3Int.one, Vector3Int.one) { }
    public TransformState(int tick, double time, Vector3 position, Vector3 rotation, Vector3Int synchronizePosition, Vector3Int synchronizeRotation)
    {
        Tick = tick;
        Time = time;
        SynchPosX = Convert.ToBoolean(Mathf.Clamp01(synchronizePosition.x));
        SynchPosY = Convert.ToBoolean(Mathf.Clamp01(synchronizePosition.y));
        SynchPosZ = Convert.ToBoolean(Mathf.Clamp01(synchronizePosition.z));
        PosX = position.x;
        PosY = position.y;
        PosZ = position.z;
        SynchRotX = Convert.ToBoolean(Mathf.Clamp01(synchronizeRotation.x));
        SynchRotY = Convert.ToBoolean(Mathf.Clamp01(synchronizeRotation.y));
        SynchRotZ = Convert.ToBoolean(Mathf.Clamp01(synchronizeRotation.z));
        RotX = rotation.x;
        RotY = rotation.y;
        RotZ = rotation.z;
    }
    public TransformState Lerp(TransformState b, TransformState t, out float tValue)
    {
        tValue = Mathf.Clamp01(Mathf.InverseLerp((float)Time, (float)b.Time, (float)t.Time));
        //Debug.Log(tValue);
        Quaternion mappedQuat = Quaternion.Lerp(GetQuatRotation(), b.GetQuatRotation(), tValue);

        TransformState result = new TransformState(
            Mathf.RoundToInt(Mathf.Lerp(Tick, b.Tick, tValue)),
            t.Time,
            Vector3.Lerp(GetPosition(), b.GetPosition(), tValue),
            mappedQuat.eulerAngles,
            t.GetSynchronizePosition(),
            t.GetSynchronizeRotation()
        );

        //Debug.Log($"tValue: {tValue}");
        //Debug.Log($"result: {result}");

        return result;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Time);
        serializer.SerializeValue(ref SynchPosX);
        serializer.SerializeValue(ref SynchPosY);
        serializer.SerializeValue(ref SynchPosZ);
        serializer.SerializeValue(ref SynchRotX);
        serializer.SerializeValue(ref SynchRotY);
        serializer.SerializeValue(ref SynchRotZ);
        if (SynchPosX) serializer.SerializeValue(ref PosX);
        if (SynchPosY) serializer.SerializeValue(ref PosY);
        if (SynchPosZ) serializer.SerializeValue(ref PosZ);
        if (SynchRotX) serializer.SerializeValue(ref RotX);
        if (SynchRotY) serializer.SerializeValue(ref RotY);
        if (SynchRotZ) serializer.SerializeValue(ref RotZ);
    }
    public int CompareTo(TransformState other)
    {
        int result = Time > other.Time ? 1 : -1;
        result = Time == other.Time ? 0 : result;

        return result;
    }
    public override string ToString()
    {
        return $"Tick: {Tick}, Time: {Time}, Position: {GetPosition()}, Rotation: {GetRotation()}";
    }
    public bool Equals(TransformState other)
    {
        return other.Tick == Tick && other.Time == Time;
    }
    public Vector3 GetPosition()
    {
        return new Vector3(PosX, PosY, PosZ);
    }
    public Vector3 GetRotation()
    {
        return new Vector3 (RotX, RotY, RotZ);
    }
    public Vector3Int GetSynchronizePosition()
    {
        return new Vector3Int(Convert.ToInt32(SynchPosX), Convert.ToInt32(SynchPosY), Convert.ToInt32(SynchPosZ));
    }
    public Vector3Int GetSynchronizeRotation()
    {
        return new Vector3Int(Convert.ToInt32(SynchRotX), Convert.ToInt32(SynchRotY), Convert.ToInt32(SynchRotZ));
    }
    public Quaternion GetQuatRotation()
    {
        return Quaternion.Euler(new Vector3(RotX, RotY, RotZ));
    }
    public static Vector3Int ToSynchVector(bool x, bool y, bool z)
    {
        return new Vector3Int(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z));
    }
    public static bool operator == (TransformState left, TransformState right)
    {
        return left.GetPosition() == right.GetPosition() && left.GetRotation() == right.GetRotation();
    }
    public static bool operator !=(TransformState left, TransformState right)
    {
        return left.GetPosition() != right.GetPosition() && left.GetRotation() != right.GetRotation();
    }
}
