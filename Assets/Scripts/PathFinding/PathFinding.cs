using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AStarPathfinding
{
	public class PathFinding
	{
		public AStarPathfindingGrid Grid { get; private set; }
		public Heap<Node> OpenNodes { get; private set; }
		public HashSet<Node> ClosedNodes { get; private set; }
		public PathFinding(AStarPathfindingGrid grid)
		{
			Grid = grid;
			OpenNodes = new(Grid.GetGridSize());
			ClosedNodes = new();
		}
		public List<Node> FindPath(Vector2 startPos, Vector2 endPos)
		{
			return FindPath(new Vector3(startPos.x, 0, startPos.y), new Vector3(endPos.x, 0, endPos.y));
		}
		public List<Node> FindPath(Vector3 startPos, Vector3 endPos)
		{
			OpenNodes.Clear();
			ClosedNodes.Clear();

			Stopwatch stopwatch = Stopwatch.StartNew();
			stopwatch.Start();

			Node startNode = Grid.GetNodeFromWorldPosition(startPos);
			Node endNode = Grid.GetNodeFromWorldPosition(endPos);

			if (!startNode.Walkable || !endNode.Walkable)
			{
				Debug.Log("Impossiblbe to create path: nodes are not walkable");
				return new List<Node>();
			}

			Node currentNode = startNode;
			OpenNodes.Add(startNode);

			int i = 0;
			while (OpenNodes.Count > 0)
			{
				currentNode = OpenNodes.RemoveFirst();
				ClosedNodes.Add(currentNode);

				if (currentNode == endNode) break;

				foreach (Node node in Grid.GetNeighborNodes(currentNode))
				{
					if (!node.Walkable || ClosedNodes.Contains(node))
					{
						continue;
					}

					int newMovementCostToNeighbour = currentNode.GCost + Node.Distance(currentNode, node) + node.MovementPenalty;
					if (!OpenNodes.Contains(node) || newMovementCostToNeighbour < node.GCost)
					{
						node.GCost = newMovementCostToNeighbour;
						node.HCost = Node.Distance(node, endNode);
						node.Parent = currentNode;

						if (!OpenNodes.Contains(node))
						{
							OpenNodes.Add(node);
						}
					}
				}

				i++;
			}

			stopwatch.Stop();
			Debug.Log($"time elapsed: {stopwatch.ElapsedMilliseconds}ms");

			return RetracePath(startNode, endNode);
		}
		static List<Node> RetracePath(Node startNode, Node endNode)
		{
			List<Node> path = new();
			Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.Parent;
			}

			path.Add(startNode);
			path.Reverse();
			return path;
		}
	}
}
