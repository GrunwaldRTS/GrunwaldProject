using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Chunk
{
	public Vector2Int Position { get; private set; }
    public Transform Parent { get; private set; }
    public ChunkDataProvider DataProvider { get; private set; }
    public GameObject MeshHolder { get; private set; }
    public Material Material { get; private set; }
    public Mesh TerrainMesh { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }
	public MeshFilter MeshFilter { get; private set; }
    public MeshCollider Collider { get; private set; }
    public float[,] HeightMap { get; set; }
    public bool Visible 
	{ 
		get => MeshHolder.activeSelf;
		set => MeshHolder.SetActive(value);
	}
    public Chunk(Vector2Int position, Material material, Transform parent, ChunkDataProvider chunkDataProvider)
    {
		Position = position;
		this.Material = material;
		Parent = parent;
		DataProvider = chunkDataProvider;

		DataProvider.RequestMesh(Position, AssignMesh);
		
		MeshHolder = new GameObject("TerrainChunk");

		MeshHolder.transform.parent = Parent.transform;
		MeshHolder.transform.position = new Vector3(Position.x, 0, Position.y);
		
		MeshRenderer = MeshHolder.AddComponent<MeshRenderer>();
		this.MeshRenderer.sharedMaterial = material;
		MeshFilter = MeshHolder.AddComponent<MeshFilter>();
		Collider = MeshHolder.AddComponent<MeshCollider>();
		MeshHolder.layer = LayerMask.NameToLayer("Ground");

		Visible = true;
    }
	private void AssignMesh(Mesh mesh, float[,] heightMap)
	{
		HeightMap = heightMap;
		TerrainMesh = mesh;
		MeshFilter.mesh = TerrainMesh;
		Collider.sharedMesh = TerrainMesh;

		EventManager.OnChunkGenerationCompleated.Invoke();
	}
	public bool IsChunkInBoundsOfPoints(List<Vector3> points)
	{
		foreach(Vector3 point in points)
		{
			if (IsPointInBoundOfChunk(point)) return true;
		}

		return false;
	}
    public bool IsChunkInBoundsOfPoints(List<Vector2> points)
    {
        foreach (Vector2 point in points)
        {
            if (IsPointInBoundOfChunk(point)) return true;
        }

        return false;
    }
    public bool IsPointInBoundOfChunk(Vector3 point)
    {
        return IsPointInBoundOfChunk(new Vector2(point.x, point.z));
    }
    public bool IsPointInBoundOfChunk(Vector2 point)
	{
		int chunkSize = DataProvider.MeshData.Size;
		int halfChunkSize = chunkSize / 2;	
        Vector4 bounds = new(Position.x + halfChunkSize, Position.x - halfChunkSize, Position.y + halfChunkSize, Position.y - halfChunkSize);

		bool xInBounds = point.x <= bounds.x && point.x >= bounds.y;
		bool yInBounds = point.y <= bounds.z && point.y >= bounds.w;

		return xInBounds && yInBounds;
	}
}
