using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkBuffer : CircullarBuffer<TransformState>
{
    //public int Index { get; set; }
    //public int TheIndex { get; set; }
    TransformState[] tempList = new TransformState[41];
    public NetworkBuffer(int size) : base(size)
    {
          
    }
    public TransformState Get(TransformState referenceT1Value)
    {
        if (referenceT1Value.Tick - 20 < 0) return default;

        int index = 0;
        for (int i = referenceT1Value.Tick - 20; i < referenceT1Value.Tick + 20; i++)
        {
            TransformState state = Get(i);

            tempList[index++] = state;
        }

        TransformState closestBehind = tempList.Where(ele => ele != default && ele.CompareTo(referenceT1Value) <= 0).LastOrDefault();

        TransformState closestInFront = tempList.Where(ele => ele != default && ele.CompareTo(referenceT1Value) >= 0).FirstOrDefault();

        float t;

        TransformState result = closestBehind.Lerp(closestInFront, referenceT1Value, out t);

        return result;
    }
}

