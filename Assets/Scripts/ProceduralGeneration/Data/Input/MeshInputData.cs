using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshInputData
{
    public int Size { get; set; }
    public int LevelOfDetail { get; set; }
    public float[,] HeightMap { get; set; }
    public float HeightMultiplier { get; set; }
    public MeshInputData(int size, int levelOfDetail, float[,] heightMap, float heightMultiplier)
    {
        Size = size;
        LevelOfDetail = levelOfDetail;
        HeightMap = heightMap;
        HeightMultiplier = heightMultiplier;
    }
}
