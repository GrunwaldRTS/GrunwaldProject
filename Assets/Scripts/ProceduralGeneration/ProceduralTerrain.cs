using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralTerrain : MonoBehaviour
{
	[SerializeField] GameObject player;
	[SerializeField] ProceduralTerrainPreset preset;

	ChunkDataProvider chunkDataProvider;

	readonly Dictionary<Vector2Int, Chunk> chunks = new();
	readonly List<Chunk> chunkList = new();
	public List<Chunk> ChunksVisibleLastUpdate { get; private set; } = new();

	int chunksLoaded = 0;
	bool areChunksLoaded;
	private void Awake()
	{
		
	}
	void MakeChunksInvisible()
	{
		ChunksVisibleLastUpdate.Clear();
		UpdateChunksVisibility();
	}
	void Start()
	{
		chunkDataProvider = new ChunkDataProvider(
			new NoiseInputData(preset.TerrainSize, preset.ChunkSize + 1, preset.LevelOfDetail, preset.HeightMultiplayer, preset.NoiseInfo),
			new MeshInputData(preset.ChunkSize, preset.LevelOfDetail, new float[0, 0], preset.HeightMultiplayer),
			preset.LakeEndHeight,
			preset.LakeHeightMultiplayer
		);

		EventManager.OnChunkGenerationCompleated.AddListener(OnChunkLoaded);
		EventManager.OnChunkMeshesInstanced.AddListener(MakeChunksInvisible);

		GenerateChunks();
	}
	void OnChunkLoaded()
	{
		chunksLoaded++;

		Debug.Log($"Chunk mesh requests: {chunksLoaded} out of {preset.TerrainSize.x * preset.TerrainSize.y} completed");

		if (chunksLoaded == preset.TerrainSize.x * preset.TerrainSize.y)
		{
			areChunksLoaded = true;

			Debug.Log("Chunks loaded!");

			EventManager.OnChunksGenerationCompleated.Invoke(chunks, chunkDataProvider.RiversPoints, chunkDataProvider.GlobalHeightMap);
		}
	}
	void GenerateChunks()
	{
		Vector2 centerOffset = new Vector2(preset.TerrainSize.x / 2, preset.TerrainSize.y / 2);

		for (int y = 0; y < preset.TerrainSize.y; y++)
		{
			for (int x = 0; x < preset.TerrainSize.x; x++)
			{
				Vector2Int position = new Vector2Int((int)((x - centerOffset.x + 0.5f) * preset.ChunkSize), (int)((y - centerOffset.y + 0.5f) * preset.ChunkSize));
				Chunk chunk = new Chunk(position, preset.ChunkMaterial, gameObject.transform, chunkDataProvider);
				chunks.Add(position, chunk);
				chunkList.Add(chunk);
			}
		}
	}
	private void Update()
	{
		if (!areChunksLoaded) return;
		UpdateChunksVisibility();
	}
	void UpdateChunksVisibility()
	{
		Vector3 playerPos = player.transform.position;

		Vector2 currentChunkPos = new Vector2(Mathf.Floor(playerPos.x / preset.ChunkSize), Mathf.Floor(playerPos.z / preset.ChunkSize));
		currentChunkPos += new Vector2(0.5f, 0.5f);

		int renderDistance = preset.RenderDistance * preset.ChunkSize;

		Chunk[] chunksInRange = chunkList.Where(chunk => Vector2.Distance(currentChunkPos, chunk.Position) <= renderDistance).ToArray();
		Chunk[] chunksOutsideRange = chunkList.Where(chunk => Vector2.Distance(currentChunkPos, chunk.Position) > renderDistance).ToArray();

		foreach(Chunk chunk in chunksOutsideRange)
		{
			if (chunk.Visible)
			{
				chunk.Visible = false;
			}
		}

		foreach (Chunk chunk in chunksInRange)
		{
			if (!chunk.Visible)
			{
				chunk.Visible = true;
			}
		}
	}
}
