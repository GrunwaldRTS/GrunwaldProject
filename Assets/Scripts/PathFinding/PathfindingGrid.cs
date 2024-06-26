using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid
{
    public ProceduralTerrainPreset TerrainPreset { get; private set; }
	public int Simplification { get; private set; } = 0;
	public int WaterMargin { get; private set; } = 5;
    public Node[,] GridNodes { get; private set; }
	int simplificationIncrement;
    int xNodesDimention, yNodesDimention;

	int penaltyMax = int.MinValue;
	int penaltyMin = int.MaxValue;

	public PathfindingGrid(ProceduralTerrainPreset terrainPreset, int simplification, int waterMargin)
	{
		TerrainPreset = terrainPreset;
		Simplification = simplification;
		WaterMargin = waterMargin;

        simplificationIncrement = (int)Mathf.Pow(2, simplification);

        xNodesDimention = terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / simplificationIncrement;
        yNodesDimention = terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / simplificationIncrement;

		Debug.Log($"nodesDimention x: {xNodesDimention}, y: {yNodesDimention}");
	
		PopulateNodes();
	}
	void PopulateNodes()
    {
		GridNodes = new Node[xNodesDimention, yNodesDimention];

		Vector2 centerOffset = new Vector2(
			(TerrainPreset.TerrainSize.x * TerrainPreset.ChunkSize / 2f),
			(TerrainPreset.TerrainSize.y * TerrainPreset.ChunkSize / 2f)
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
				bool walkable = hit.point.y >= TerrainPreset.WaterLevel + WaterMargin;
				int penalty = walkable ? 0 : 80;

				GridNodes[x, y] = new Node(walkable, position, worldPos, penalty);
			}
		}

		BlurPenaltyMap(4);
	}
	public int GetGridSize() => GridNodes.Length;
	public Node GetNodeFromWorldPosition(Vector3 position)
	{
		Vector2 terrainSize = TerrainPreset.TerrainSize * TerrainPreset.ChunkSize;
		Vector2 procent = new((position.x + (terrainSize.x / 2)) / terrainSize.x, (position.z + (terrainSize.x / 2)) / terrainSize.x);
		Vector2Int pos = new(Mathf.RoundToInt(procent.x * xNodesDimention), Mathf.RoundToInt(procent.y * yNodesDimention));

		pos.x = pos.x >= xNodesDimention ? xNodesDimention - 1 : pos.x;
        pos.y = pos.y >= xNodesDimention ? yNodesDimention - 1 : pos.y;

        return GridNodes[pos.x, pos.y];
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
				penaltiesHorizontalPass[0, y] += GridNodes[sampleX, y].MovementPenalty;
			}

			for (int x = 1; x < xNodesDimention; x++)
			{
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, xNodesDimention);
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, xNodesDimention - 1);

				penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - GridNodes[removeIndex, y].MovementPenalty + GridNodes[addIndex, y].MovementPenalty;
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
				GridNodes[x, y].MovementPenalty = blurredPenalty;

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
				nodes.Add(GridNodes[pos.x, pos.y]);
			}
		}

		return nodes;
	}
	//private void OnDrawGizmos()
	//{
	//	//if (gridNodes != null)
	//	//{
	//	//	foreach (Node node in gridNodes)
	//	//	{
	//	//		Gizmos.color = node.Walkable ? Color.white : Color.red;
	//	//		Gizmos.DrawWireCube(node.WorldPos + new Vector3(0.5f, 0, 0.5f), new Vector3(0.9f, /*Mathf.Lerp(1, 5, Mathf.InverseLerp(penaltyMin, penaltyMax, node.MovementPenalty))*/node.MovementPenalty, 0.9f));
	//	//	}
	//	//}
	//}
}
