using System;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CircullarBuffer<T>
{
    public int Size { get; protected set; }
    public T[] Array { get; protected set; }
    public CircullarBuffer(int size)
    {
        Size = size;
        Array = new T[size];
    }
    public void Set(T ele, int index)
    {
        Array[index % Size] = ele;
    }
    public T Get(int index)
    {
        return Array[index % Size];
    }
}