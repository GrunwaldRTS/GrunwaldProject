using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PathFinding
{
	public Grid Grid { get; private set; }
	public Heap<Node> OpenNodes { get; private set; }
	public HashSet<Node> ClosedNodes { get; private set; }
    public PathFinding(Grid grid)
    {
		Grid = grid;
		OpenNodes = new(Grid.GridSize());
		ClosedNodes = new();
	}
	public List<Node> FindPath(Vector3 startPos, Vector3 endPos)
	{
		OpenNodes.Clear();
		ClosedNodes.Clear();

		//Heap<Node> OpenNodes = new(Grid.GridSize());
		//HashSet<Node> ClosedNodes = new();

		Stopwatch stopwatch = Stopwatch.StartNew();
		stopwatch.Start();

		Node startNode = Grid.GetNodeFromWorldPosition(startPos);
		//Debug.DrawRay(startNode.WorldPos, Vector3.up * 1000f, Color.yellow, 1000f);

		Node endNode = Grid.GetNodeFromWorldPosition(endPos);
		//Debug.DrawRay(endNode.WorldPos, Vector3.up * 1000f, Color.yellow, 1000f);

		if (!startNode.Walkable || !endNode.Walkable)
		{
			Debug.Log("Impossiblbe to create path: nodes are not walkable");
			return new List<Node>();
		}

		Node currentNode = startNode;
		OpenNodes.Add(startNode);

		//foreach (Node node in grid.GetNeighborNodes(startNode))
		//{
		//	Debug.DrawRay(node.WorldPos, Vector3.up * 100, Color.yellow, 1000);
		//}
		int i = 0;
		while (OpenNodes.Count > 0)
		{
			currentNode = OpenNodes.RemoveFirst();
			ClosedNodes.Add(currentNode);

			if (currentNode == endNode) break;

			foreach (Node node in Grid.GetNeighborNodes(currentNode))
			{
				if(!node.Walkable || ClosedNodes.Contains(node))
				{	
					continue;
				}

				int newMovementCostToNeighbour = currentNode.GCost + Node.Distance(currentNode, node);
				if (!OpenNodes.Contains(node) || newMovementCostToNeighbour < node.GCost)
				{
					node.GCost = newMovementCostToNeighbour;
					node.HCost = Node.Distance(node, endNode);
					node.Parent = currentNode;

					//Debug.DrawRay(node.WorldPos, Vector3.up * 100, Color.yellow, 1000);
					if (!OpenNodes.Contains(node))
					{
						OpenNodes.Add(node);
					}
				}
			}
			//Debug.Log($"iteration: {i}");
			//foreach(Node node in openNodes)
			//{
			//	Debug.Log($"pos: {node.WorldPos}");
			//}

			//if (i == 10) break;

			i++;
		}

		stopwatch.Stop();
		Debug.Log($"time elapsed: {stopwatch.ElapsedMilliseconds}ms");

		return RetracePath(startNode, endNode);
		//return new List<Node>();
	}
	static List<Node> RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new();
		Node currentNode = endNode;

		//Debug.Log($"startPos: {startNode.Pos}");

		while(currentNode != startNode)
		{
			//Debug.Log($"pos: {currentNode.Pos}");
			path.Add(currentNode);
			currentNode = currentNode.Parent;
		}

		path.Add(startNode);
		path.Reverse();
		return path;
	}
}
