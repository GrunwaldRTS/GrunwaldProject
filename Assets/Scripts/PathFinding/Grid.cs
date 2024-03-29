using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
	[SerializeField][Range(0, 5)] int simplification = 0;
	[SerializeField][Range(0, 10)] int waterMargin = 5;
	int simplificationIncrement;
    [SerializeField] ProceduralTerrainPreset terrainPreset;
    int xNodesDimention, yNodesDimention;
	//int i = 0;
    Node[,] gridNodes;

	int penaltyMax = int.MinValue;
	int penaltyMin = int.MaxValue;

	private void Awake()
	{
		simplificationIncrement = (int)Mathf.Pow(2, simplification);

        xNodesDimention = terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / simplificationIncrement;
        yNodesDimention = terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / simplificationIncrement;

	
		Debug.Log($"nodesDimention x: {xNodesDimention}, y: {yNodesDimention}");


		EventManager.OnChunksGenerationCompleated.AddListener(PopulateNodes);
	}
	void PopulateNodes(ChunkDataProvider chunkDataProvider, Dictionary<Vector2Int, Chunk> chunks, Vector2Int[] riversPoints, Dictionary<Vector2Int, float> globalHeightMap)
    {
		gridNodes = new Node[xNodesDimention, yNodesDimention];

		Vector2 centerOffset = new Vector2(
			(terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / 2f),
			(terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / 2f)
		);
		Debug.Log($"centerOffset: {centerOffset}");

		for (int x = 0; x < xNodesDimention; x++)
		{
			for (int y = 0; y < yNodesDimention; y++)
			{
				Vector2Int position = new Vector2Int(x, y);
				Vector3 worldPos = new(x * simplificationIncrement - centerOffset.x, 70, y * simplificationIncrement - centerOffset.y);
				Ray ray = new Ray(new Vector3(worldPos.x + 0.5f, worldPos.y, worldPos.z + 0.5f), Vector3.down);

				Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground"));
				worldPos.y = hit.point.y;
				//Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.gray, 1000f);
				bool walkable = hit.point.y >= terrainPreset.WaterLevel + waterMargin;
				int penalty = walkable ? 0 : 80;

				gridNodes[x, y] = new Node(walkable, position, worldPos, penalty);
			}
		}

		BlurPenaltyMap(4);
	}
	public int GetGridSize() => gridNodes.Length;
	public Node GetNodeFromWorldPosition(Vector3 position)
	{
		Vector2 terrainSize = terrainPreset.TerrainSize * terrainPreset.ChunkSize;
		Vector2 procent = new((position.x + (terrainSize.x / 2)) / terrainSize.x, (position.z + (terrainSize.x / 2)) / terrainSize.x);
		//procent.x = Mathf.InverseLerp(0, simplificationIncrement, procent.x);
		//procent.y = Mathf.InverseLerp(0, simplificationIncrement, procent.y);
		Vector2Int pos = new(Mathf.RoundToInt(procent.x * xNodesDimention), Mathf.RoundToInt(procent.y * yNodesDimention));

		//Debug.Log($"procent: {procent}");
		//Debug.Log($"nodesDimention x: {xNodesDimention}, y: {yNodesDimention}");

		return gridNodes[pos.x, pos.y];
	}
	void BlurPenaltyMap(int blurSize)
	{
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = blurSize;

		int[,] penaltiesHorizontalPass = new int[xNodesDimention, yNodesDimention];
        int[,] penaltiesVerticalPass = new int[xNodesDimention, yNodesDimention];

		for (int y = 0; y < yNodesDimention; y++)
		{
			for (int x = -kernelExtents; x <= kernelExtents; x++)
			{
				int sampleX = Mathf.Clamp(x, 0, kernelExtents);
				penaltiesHorizontalPass[0, y] += gridNodes[sampleX, y].MovementPenalty;
			}

			for (int x = 1; x < xNodesDimention; x++)
			{
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, xNodesDimention);
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, xNodesDimention - 1);

				penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - gridNodes[removeIndex, y].MovementPenalty + gridNodes[addIndex, y].MovementPenalty;
			}
		}

        for (int x = 0; x < xNodesDimention; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

			for (int y = 1; y < yNodesDimention; y++)
			{
				int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, yNodesDimention);
				int addIndex = Mathf.Clamp(y + kernelExtents, 0, yNodesDimention - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
				int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
				gridNodes[x, y].MovementPenalty = blurredPenalty;

				if (penaltyMax < blurredPenalty)
				{
					penaltyMax = blurredPenalty;
				}
				if (penaltyMin > blurredPenalty)
				{
					penaltyMin = blurredPenalty;
				}
			}
        }
    }
	public List<Node> GetNeighborNodes(Node node)
	{
		List<Node> nodes = new();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0) continue;
				
				Vector2Int pos = new(x + node.Pos.x, y + node.Pos.y);
				if (pos.x < 0 || pos.x >= xNodesDimention || pos.y < 0 || pos.y >= yNodesDimention) continue;

				//Debug.Log($"x{pos.x} : {pos.y}");
				//Debug.Log($"dimentions: {pos}");
				//Debug.Log($"node pos: {node.Pos}");
				//Debug.Log($"node world pos: {node.WorldPos}");
				nodes.Add(gridNodes[pos.x, pos.y]);
			}
		}

		return nodes;
	}
	private void OnDrawGizmos()
	{
		if (gridNodes != null)
		{
			foreach (Node node in gridNodes)
			{
				Gizmos.color = node.Walkable ? Color.white : Color.red;
				Gizmos.DrawWireCube(node.WorldPos + new Vector3(0.5f, 0, 0.5f), new Vector3(0.9f, /*Mathf.Lerp(1, 5, Mathf.InverseLerp(penaltyMin, penaltyMax, node.MovementPenalty))*/node.MovementPenalty, 0.9f));
			}
		}
	}
}
