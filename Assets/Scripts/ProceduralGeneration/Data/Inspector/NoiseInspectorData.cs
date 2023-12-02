using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NoiseInspectorData
{
	public int Seed;
	[Range(0.1f, 200f)] public float Scale;
	[Range(1, 10)] public int Octaves;
	[Range(0.1f, 10f)] public float Lacunarity;
	[Range(0.1f, 1f)] public float Persistance;
	public int RiversCount;
	public int RiversWidth;
	public AnimationCurve RiversHeightCurve;
}
