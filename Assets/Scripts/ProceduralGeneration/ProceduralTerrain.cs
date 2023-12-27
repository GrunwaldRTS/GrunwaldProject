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

	int chunksLoaded = 0;
	bool areChunksLoaded;
	void MakeChunksInvisible()
	{
		UpdateChunksVisibility();
	}
	void Start()
	{
		Application.targetFrameRate = 240;

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

			EventManager.OnChunksGenerationCompleated.Invoke(chunkDataProvider, chunks, chunkDataProvider.RiversPoints, chunkDataProvider.GlobalHeightMap);
		}
	}
	void GenerateChunks()
	{
		Vector2 centerOffset = new Vector2((preset.TerrainSize.x * preset.ChunkSize / 2f) - preset.ChunkSize / 2f, (preset.TerrainSize.y * preset.ChunkSize / 2f) - preset.ChunkSize / 2f);
		Debug.Log($"centerOffset: {centerOffset}");
		for (int y = 0; y < preset.TerrainSize.y; y++)
		{
			for (int x = 0; x < preset.TerrainSize.x; x++)
			{
				Vector2Int position = new Vector2Int((int)(x * preset.ChunkSize - centerOffset.x), (int)(y * preset.ChunkSize - centerOffset.y));
				Chunk chunk = new Chunk(position, preset.ChunkMaterial, gameObject.transform, chunkDataProvider);
				chunks.Add(position, chunk);
			}
		}
	}
	private void Update()
	{
		if (!areChunksLoaded) return;
		//UpdateChunksVisibility();
	}
	void UpdateChunksVisibility()
	{
		//Debug.Log("update");
		Vector3 playerPos = player.transform.position;

		int renderDistance = preset.RenderDistance * preset.ChunkSize;

		foreach (KeyValuePair<Vector2Int, Chunk> keyValuePair in chunks)
		{
			Chunk chunk = keyValuePair.Value;

			//Debug.Log($"{Vector2.Distance(chunk.Position, currentChunkPos)} < {renderDistance}");
			if (Vector2.Distance(chunk.Position, new Vector2(playerPos.x, playerPos.z)) < renderDistance)
			{
				if (!chunk.Visible)
				{
					//Debug.Log("visible");
					chunk.Visible = true;
				}
			}
			else
			{
				if (chunk.Visible)
				{
					//Debug.Log("invisible");
					chunk.Visible = false;
				}
			}
		}
	}
}
