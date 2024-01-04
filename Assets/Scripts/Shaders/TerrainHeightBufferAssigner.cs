using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightBufferAssigner : MonoBehaviour
{
    [SerializeField] BiomePreset data;
	ComputeBuffer heightsBuffer;

	private void Start()
	{
		heightsBuffer = new(data.Heights.Length, Height.Size());
		heightsBuffer.SetData(data.Heights);
		data.TerrainMaterial.SetInt("_HeightsCount", data.Heights.Length);
		data.TerrainMaterial.SetBuffer("_Heights", heightsBuffer);
	}
	private void OnDisable()
	{
		heightsBuffer.Release();
		heightsBuffer = null;
	}
}
