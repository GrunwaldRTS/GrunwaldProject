using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using UnityEditor.PackageManager;
using UnityEngine;

public class ChunkDataProvider
{
    //noise
    public NoiseInputData NoiseData { get; private set; }
    //mesh
    public MeshInputData MeshData { get; private set; }
    public float LakeEndHeight { get; private set; }
    public float LakeHeightMultiplier { get; private set; }
    //threading
    public int ChunkCount { get; private set; }
    public Vector2Int[] RiversPoints { get; private set; }
    public Dictionary<Vector2Int, float> GlobalHeightMap { get; set; }

    Queue<ThreadInfo<MeshNoisePair, Mesh, float[,]>> meshActionPairsQueue = new();
    public ChunkDataProvider(NoiseInputData noiseData, MeshInputData meshData, float lakeEndHeight, float lakeHeightMultiplier)
    {
        NoiseData = noiseData;
        MeshData = meshData;
		LakeEndHeight = lakeEndHeight;
        LakeHeightMultiplier = lakeHeightMultiplier;

        new GameObject("CDPUpdate").AddComponent<CDPUpdate>().CDP = this;

        RiversPoints = Noise.GetRiversPoints(noiseData);
        GlobalHeightMap = new();
    }
    public void RequestMesh(Vector2Int offset, Action<Mesh, float[,]> callback)
    {
		new Thread(() =>
		{
            Dictionary<Vector2Int, float> heightMap;

            float[,] heightMap2 = Noise.GenerateChunkMap(NoiseData, offset, RiversPoints, out heightMap);

			lock (meshActionPairsQueue)
			{
                meshActionPairsQueue.Enqueue(
                    new ThreadInfo<MeshNoisePair, Mesh, float[,]>
					{
                        RequestedData = new (
                            MeshGenerator.GenerateTerrainChunkData(new MeshInputData(MeshData.Size, NoiseData.LevelOfDetail, heightMap2, MeshData.HeightMultiplier), LakeEndHeight, LakeHeightMultiplier),
                            heightMap2
                            ),
                        CallBack = callback
                    }
                );
			}

            lock (GlobalHeightMap)
            {
                foreach(KeyValuePair<Vector2Int, float> pair in heightMap)
                {
                    if (!GlobalHeightMap.ContainsKey(pair.Key))
                    {
                        GlobalHeightMap[pair.Key] = pair.Value;
                    }
                }
            }

		}).Start();
	}
    public void UpdateRequestQueue()
    {
        if(meshActionPairsQueue.Count > 0)
        {
            for (int i = 0; i < meshActionPairsQueue.Count; i++)
            {
                ThreadInfo<MeshNoisePair, Mesh, float[,]> threadInfo = meshActionPairsQueue.Dequeue();

				threadInfo.CallBack(threadInfo.RequestedData.MeshData.CreateMesh(), threadInfo.RequestedData.HeightMap);
			}
        }
    }
    public void ChangeVertex(Vector2Int vertexPos, Func<float, float> func)
    {
        if (!GlobalHeightMap.ContainsKey(vertexPos)) 
        {
            Debug.Log("Changing the vertex failed: VERTEX DOESN'T EXISTS!");
            return;
        }
        
        float value = func(GlobalHeightMap[vertexPos]);
        GlobalHeightMap[vertexPos] = value;

        Vector2Int chunkCoords = new Vector2Int(vertexPos.x / MeshData.Size, vertexPos.y / MeshData.Size);
        chunkCoords *= MeshData.Size;
        Debug.DrawRay(new Vector3(vertexPos.x, value, vertexPos.y), Vector3.up * 500f, Color.black, 1000);
        Debug.DrawRay(new Vector3(chunkCoords.x, value, chunkCoords.y), Vector3.up * 1000f, Color.black, 1000);
    }
}
public struct MeshNoisePair
{
    public MeshData MeshData { get; set; }
    public float[,] HeightMap { get; set; }
    public MeshNoisePair(MeshData meshData, float[,] heightMap)
    {
        MeshData = meshData;
        HeightMap = heightMap;
    }
}
public struct ThreadInfo<T, T1>
{
    public T RequestedData { get; set; }
    public Action<T1> CallBack { get; set; }
}
public struct ThreadInfo<T, T1, T2>
{
	public T RequestedData { get; set; }
	public Action<T1, T2> CallBack { get; set; }
}
