using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
	[SerializeField] GameObject camera;
    [SerializeField] ProceduralTerrainPreset terrainPreset;
    int xNodesDimention, yNodesDimention;

    Node[,] nodes;
	private void Awake()
	{
        xNodesDimention = terrainPreset.TerrainSize.x * terrainPreset.ChunkSize;
        yNodesDimention = terrainPreset.TerrainSize.y * terrainPreset.ChunkSize;

		EventManager.OnChunksGenerationCompleated.AddListener(PopulateNodes);
	}
	void PopulateNodes(ChunkDataProvider chunkDataProvider, Dictionary<Vector2Int, Chunk> chunks, Vector2Int[] riversPoints, Dictionary<Vector2Int, float> globalHeightMap)
    {
		nodes = new Node[xNodesDimention, yNodesDimention];

		Vector2 centerOffset = new Vector2(
			(terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / 2f),
			(terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / 2f)
		);
		Debug.Log($"centerOffset: {centerOffset}");

		for (int x = 0; x < xNodesDimention; x++)
		{
			for (int y = 0; y < yNodesDimention; y++)
			{
				Vector2Int position = new Vector2Int(x - (int)centerOffset.x, y - (int)centerOffset.y);
				Ray ray = new Ray(new Vector3(position.x + 0.5f, 70, position.y + 0.5f), Vector3.down);

				Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground"));
				//Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.gray, 1000f);
				bool walkable = hit.point.y >= terrainPreset.WaterLevel;

				nodes[x, y] = new Node(walkable, position, hit.point.y);
			}
		}
	}
	private class Node
	{
        public bool Walkable { get; private set; }
        public Vector2Int Pos { get; private set; }
        public float Height { get; private set; }
        public Node(bool walkable, Vector2Int pos, float height)
        {
            Walkable = walkable;
            Pos = pos;
			Height = height;
        }
    }
	private void OnDrawGizmos()
	{
		if (nodes != null)
		{
			foreach (Node node in nodes)
			{
				Vector3 pos = new Vector3(node.Pos.x, node.Height, node.Pos.y);
				Gizmos.DrawWireCube(pos, new Vector3(0.9f, 0.9f, 0.9f));
			}
		}
	}
}
