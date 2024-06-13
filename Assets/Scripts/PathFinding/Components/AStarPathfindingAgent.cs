using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using AStarPathfinding;
using Debug = UnityEngine.Debug;
using UnityEngine.Playables;
using UnityEditor;

[RequireComponent(typeof(Transform))]
[DisallowMultipleComponent]
public class AStarPathfindingAgent : MonoBehaviour
{
    [SerializeField][Range(0, 1.5f)] float speed = 0.8f;
    [SerializeField] Vector3 agentCeterOffset;
    public List<Node> CurrentPath { get; private set; }
    public Vector3 CurrentDestination { get; private set; }
    public Node CurrentDestinationNode { get; private set; }

    bool hasBeedPlaced;
    PathFinding pathfinding;
    Coroutine tracePath;
    private void Awake()
    {
        EventManager.OnGeneratedPathfindingGrid.AddListener(Initialize);
    }
    void Initialize()
    {
        Node node = AStarPathfindingGrid.Instance.GetNodeFromWorldPosition(transform.position);

        if (node is null) return;

        hasBeedPlaced = true;

        transform.position = node.WorldPos + agentCeterOffset + new Vector3(0.5f, 0, 0.5f);

        pathfinding = new PathFinding(AStarPathfindingGrid.Instance);
    }
    public List<Node> FindPath(Vector2 startPos, Vector2 endPos)
    {
        return pathfinding.FindPath(new Vector3(startPos.x, 0, startPos.y), new Vector3(endPos.x, 0, endPos.y));
    }
    public List<Node> FindPath(Vector3 startPos, Vector3 endPos)
    {
        return pathfinding.FindPath(startPos, endPos);
    }
    public void SetDestination(Vector3 destination)
    {
        if (!hasBeedPlaced)
        {
            Debug.LogError("Pathfinding Agent was not placed correctly");
            return;
        }

        ResetDestination();

        CurrentDestination = destination;
        CurrentDestinationNode = AStarPathfindingGrid.Instance.GetNodeFromWorldPosition(destination);
        CurrentPath = pathfinding.FindPath(transform.position, destination);

        tracePath = StartCoroutine(TracePath());
    }
    public void ResetDestination()
    {
        if (tracePath is not null) StopCoroutine(tracePath);
        ResetState();
    }
    void ResetState()
    {
        CurrentPath = null;
        CurrentDestination = Vector3.zero;
        CurrentDestinationNode = null;
    }
    float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }
    IEnumerator TracePath()
    {
        int index = 0;
        Vector3 currentNode = transform.position - agentCeterOffset;
        Vector3 nextNode = CurrentPath[index + 1].WorldPos;

        foreach(Node node in CurrentPath)
        {
            Debug.DrawRay(node.WorldPos, Vector3.up * 10f, Color.red, 1000f);
        }

        while (true)
        {
            float t = InverseLerp(currentNode, nextNode, transform.position - agentCeterOffset);

            //Debug.Log(t);

            if (t == 1)
            {
                index++;

                if (index == CurrentPath.Count - 1)
                {
                    ResetDestination();
                    yield break;
                }

                currentNode = CurrentPath[index].WorldPos;
                nextNode = CurrentPath[index + 1].WorldPos;

                transform.position = currentNode + agentCeterOffset;

                t = 0;
            }

            Vector3 nextPosition = Vector3.Lerp(currentNode, nextNode, t + speed * Time.deltaTime);
            transform.position = nextPosition + agentCeterOffset;

            yield return null;
        }
    }
}
