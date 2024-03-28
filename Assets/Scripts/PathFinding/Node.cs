using UnityEngine;

public class Node : IHeapElement<Node>
{
	public bool Walkable { get; private set; }
	public Vector2Int Pos { get; private set; }
    public Vector3 WorldPos { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get => GCost + HCost; }
    public Node Parent { get; set; }
	public int HeapIndex { get; set; }
    public int MovementPenalty { get; set; }

    public Node(bool walkable, Vector2Int pos, Vector3 worldPos, int penalty)
	{
		Walkable = walkable;
		Pos = pos;
		WorldPos = worldPos;
		MovementPenalty = penalty;
	}
	public static int Distance(Node node1, Node node2)
	{
		Vector2Int posDiff = new Vector2Int(Mathf.Abs(node1.Pos.x - node2.Pos.x), Mathf.Abs(node1.Pos.y - node2.Pos.y));

		if(posDiff.x > posDiff.y) return 14 * posDiff.y + 10 * (posDiff.x - posDiff.y);
		else return 14 * posDiff.x + 10 * (posDiff.y - posDiff.x);
	}

	public int CompareTo(Node other)
	{
		int compare = FCost.CompareTo(other.FCost);

		if(compare == 0)
		{
			compare = HCost.CompareTo(other.HCost);
		}
		return -compare;
	}
}