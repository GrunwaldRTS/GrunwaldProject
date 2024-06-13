using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
	public static readonly Event<ChunkDataProvider, Dictionary<Vector2Int, Chunk>, Vector2Int[], Dictionary<Vector2Int, float>> OnChunksGenerationCompleated = new();
	public static readonly Event OnChunkGenerationCompleated = new();
	public static readonly Event OnChunkMeshesInstanced = new();
	public static readonly Event OnPathPresetValidate = new();
	public static readonly Event OnGeneratedPathfindingGrid = new();

	public static readonly Event OnSignedIn = new();
	public static readonly Event OnJoinedLobby = new();
    public static readonly Event OnLeftLobby = new();
}
