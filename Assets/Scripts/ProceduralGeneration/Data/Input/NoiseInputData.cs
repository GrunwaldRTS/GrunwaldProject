using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NoiseInputData
{
    public Vector2Int TerrainSize { get; set; }
    public int Size { get; set; }
    public int LevelOfDetail { get; set; }
    public int Seed { get; set; }
    public float Scale { get; set; }
    public int Octaves { get; set; }
    public float Lacunarity { get; set; }
    public float Persistance { get; set; }
    public int RiversCount { get; set; }
    public int RiversWidth { get; set; }
    public AnimationCurve RiversHeightCurve { get; set; }
	public float HeightMultiplier { get; set; }
	public NoiseInputData(Vector2Int terrainSize, int size, int levelOfDetail, int seed, float scale, int octaves, float lacunarity, float persistance, int riversCount, int riversWidth, AnimationCurve riversHeightCurve, float heightMultiplier)
	{
        TerrainSize = terrainSize;
		Size = size;
        LevelOfDetail = levelOfDetail;
		Seed = seed;
		Scale = scale;
		Octaves = octaves;
		Lacunarity = lacunarity;
		Persistance = persistance;
        RiversCount = riversCount;
        RiversWidth = riversWidth;
        RiversHeightCurve = riversHeightCurve;
        HeightMultiplier = heightMultiplier;
	}
	public NoiseInputData(Vector2Int terrainSize, int size, int levelOfDetail, float heightMultiplier, NoiseInspectorData noiseInfo)
    {
        TerrainSize = terrainSize;
        Size = size;
        LevelOfDetail = levelOfDetail;
        Seed = noiseInfo.Seed;
        Scale = noiseInfo.Scale;
        Octaves = noiseInfo.Octaves;
        Lacunarity = noiseInfo.Lacunarity;
        Persistance = noiseInfo.Persistance;
        RiversCount = noiseInfo.RiversCount;
        RiversWidth = noiseInfo.RiversWidth;
        HeightMultiplier = heightMultiplier;
        RiversHeightCurve = noiseInfo.RiversHeightCurve;
	}
}
