using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Height
{
	public float startHeight;
	public float endHeight;
	public Color color;

	public static int Size()
	{
		return sizeof(float) * 2 + sizeof(float) * 4;
	}
}

