using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProceduralTerrainPreset", menuName = "ScriptableObjects/ProceduralTerrainPreset")]
public class ProceduralTerrainPreset : ScriptableObject
{
	[Header("Terrain")]
	[SerializeField] Vector2Int _terrainSize = new Vector2Int(2, 2);
	[SerializeField][Range(2, 10)] int _renderDistance;
	[Header("Chunk")]
	[SerializeField] Material _chunkMaterial;
	[SerializeField][Range(0, 6)] int _levelOfDetail;
	[SerializeField][Range(1f, 100f)] float _heightMultiplayer;
	[SerializeField][Range(0, 2f)] float _lakeEndHeight;
	[SerializeField][Range(0, 100f)] float _lakeHeightMultiplayer;
	[Header("Noise")]
	[SerializeField] NoiseInspectorData _noiseInfo;

    public Vector2Int TerrainSize { get => _terrainSize; }
	public int RenderDistance { get => _renderDistance; }
	public Material ChunkMaterial { get => _chunkMaterial; }
	public int LevelOfDetail { get => _levelOfDetail; }
	public float HeightMultiplayer { get => _heightMultiplayer; }
	public float LakeEndHeight { get => _lakeEndHeight; }
    public float LakeHeightMultiplayer { get => _lakeHeightMultiplayer; }
    public NoiseInspectorData NoiseInfo { get => _noiseInfo; }
	public int ChunkSize { get; } = 238;
}