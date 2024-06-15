using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using AStarPathfinding;
using Debug = UnityEngine.Debug;
using UnityEngine.Playables;
using UnityEditor;
using System;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class AStarPathfindingAgent : MonoBehaviour
{
    [SerializeField][Range(0, 1000f)] float speed = 600f;
    [SerializeField] Vector3 agentCeterOffset;
    [SerializeField][Range(0, 10f)] float walkDirectlyToDestinationDistance = 1f;
    public bool HasPath { get; private set; }
    public List<Node> CurrentPath { get; private set; }
    public Vector3 CurrentDestination { get; private set; }
    public Node CurrentDestinationNode { get; private set; }

    public Action OnReachDestination;

    bool hasBeedPlaced;
    bool isNextToTarget;

    Vector3 currentNode;
    int pathIndex;

    PathFinding pathfinding;

    Rigidbody rBody;
    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        if (AStarPathfindingGrid.Instance.IsGridGenerated)
        {
            Initialize();
        }
    }
    private void OnEnable()
    {
        EventManager.OnGeneratedPathfindingGrid.AddListener(Initialize);
    }
    private void OnDisable()
    {
        EventManager.OnGeneratedPathfindingGrid.RemoveListener(Initialize);
    }
    void Initialize()
    {
        Debug.Log("initialize");

        Node node = AStarPathfindingGrid.Instance.GetNodeFromWorldPosition(transform.position);

        Debug.Log($"initialize: {node}");

        if (node is null) return;

        hasBeedPlaced = true;

        transform.position = node.WorldPos + agentCeterOffset + new Vector3(0.5f, 0, 0.5f);

        pathfinding = new PathFinding(AStarPathfindingGrid.Instance);
    }
    private void Update()
    {
        if (!HasPath) return;

        if (!isNextToTarget && Vector3.Distance(transform.position - agentCeterOffset, currentNode) < 1.5f)
        {
            pathIndex++;

            currentNode = CurrentPath[pathIndex].WorldPos;
        }

        if (Vector3.Distance(transform.position, CurrentDestination) < walkDirectlyToDestinationDistance)
        {
            currentNode = CurrentDestination;

            isNextToTarget = true;
        }

        if (Vector3.Distance(transform.position - agentCeterOffset, CurrentDestination) < 0.1f)
        {
            Debug.Log("At destination");

            ResetDestination();

            OnReachDestination();

            return;
        }
    }
    private void FixedUpdate()
    {
        if (!HasPath) return;

        Vector3 posToDest = (currentNode - (transform.position - agentCeterOffset)).normalized;
        rBody.AddForce(speed * posToDest);
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

        Debug.Log(CurrentPath.Count);

        Debug.DrawRay(transform.position, Vector3.up * 10f, Color.yellow, 1000f);
        Debug.DrawRay(destination, Vector3.up * 10f, Color.blue, 1000f);

        Debug.Log(pathIndex);

        currentNode = CurrentPath[pathIndex++].WorldPos;
        foreach (Node node in CurrentPath)
        {
            Debug.DrawRay(node.WorldPos, Vector3.up * 10f, Color.red, 1000f);
        }

        HasPath = true;
    }
    public void ResetDestination()
    {
        HasPath = false;
        isNextToTarget = false;
        CurrentPath = null;
        currentNode = Vector3.zero;
        pathIndex = 0;
        CurrentDestination = Vector3.zero;
        CurrentDestinationNode = null;
    }
}
