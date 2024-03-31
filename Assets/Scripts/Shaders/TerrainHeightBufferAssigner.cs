using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightBufferAssigner : MonoBehaviour
{
    [SerializeField] BiomePreset data;
	ComputeBuffer heightsBuffer;

	private void Awake()
	{
		
	}
	private void OnDisable()
	{
		//heightsBuffer.Release();
		//heightsBuffer = null;
	}
}
