using System;
using UnityEngine;

[Serializable]
public struct GrassInspectorData
{
	public Mesh mesh;
	public Material material;
	public Vector3 scale;
	[Range(1, 10)]public int density;
}