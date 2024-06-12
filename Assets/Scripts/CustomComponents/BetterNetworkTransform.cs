using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

[RequireComponent(typeof(Transform))]
[DisallowMultipleComponent]
public class BetterNetworkTransform : NetworkBehaviour
{
    [Header("Synchronization")]
    [Header("Position")]
    [SerializeField] bool posX = true;
    [SerializeField] bool posY = true;
    [SerializeField] bool posZ = true;
    [Header("Rotation")]
    [SerializeField] bool rotX = false;
    [SerializeField] bool rotY = true;
    [SerializeField] bool rotZ = false;
    [Header("Transform")]
    [SerializeField] bool usesNavmeshAgent = false;
    [Header("Visualization")]
    [SerializeField] bool visualizeReconciliation = false;

    NavMeshAgent agent;

    Transform serverCube;
    Transform clientCube;

    //client
    public NetworkBuffer ClientBuffer { get; private set; }
    public CircullarBuffer<InputState> ServerApprovedInputStates { get; private set; }
    public NetworkBuffer ServerUpdatesBuffer { get; private set; }
    public TransformState LastServerState { get; private set; }
    public TransformState LastReconciledState { get; private set; }
    bool clientTickPending = false;
    
    //server
    bool serverTickPnding = false;
    private void Awake()
    {
        ServerUpdatesBuffer = new(2048);
        ClientBuffer = new(2048);
        ServerApprovedInputStates = new(2048);
        GetCubesTransforms();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.NetworkTickSystem.Tick += ServerTick;
        }
        else
        {
            NetworkManager.Singleton.NetworkTickSystem.Tick += ClientTick;
        }

        if (usesNavmeshAgent)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.NetworkTickSystem.Tick -= ServerTick;
        }
        else
        {
            NetworkManager.Singleton.NetworkTickSystem.Tick -= ClientTick;
        }

    }
    void ServerTick()
    {
        if (!IsServer) return;

        serverTickPnding = true;
    }
    public void ClientTick()
    {
        if (!IsClient) return;

        clientTickPending = true;
    }
    private void Update()
    {
        HandleServerTick();
        HandleClientTick();
        HandleInterpolation();
    }
    void HandleServerTick()
    {
        if (!serverTickPnding) return;

        TransformState ts = GetTransformState();
        UpdateStateClientRpc(ts);

        if (!usesNavmeshAgent) return;
        InputState inputState = NetworkManagement.Instance.GetInputState(agent.destination, Quaternion.identity);
        UpdateInputStateClientRpc(inputState);
    }
    [ClientRpc]
    void UpdateStateClientRpc(TransformState newState)
    {
        //if (IsServer) return;

        LastServerState = newState;
        ServerUpdatesBuffer.Set(newState, newState.Tick);
    }
    [ClientRpc]
    void UpdateInputStateClientRpc(InputState newState)
    {
        if (!usesNavmeshAgent) return;
        ServerApprovedInputStates.Set(newState, newState.Tick);
    }
    [ClientRpc]
    public void OutsideUpdateInputStateClientRpc(InputState newState)
    {
        if (usesNavmeshAgent) return;
        ServerApprovedInputStates.Set(newState, newState.Tick);
    }
    void HandleClientTick()
    {
        if (!IsClient) return;
        if (!clientTickPending) return;

        TransformState currentClientState = GetTransformState();
        ClientBuffer.Set(currentClientState, currentClientState.Tick);

        HandleReconcolidation();
    }
    void HandleReconcolidation()
    {
        if (!IsClient) return;
        if (LastServerState == null) return;

        TransformState lastServerState = LastServerState;

        int tick = lastServerState.Tick;
        TransformState serverState = ServerUpdatesBuffer.Get(tick);
        TransformState clientState = ClientBuffer.Get(tick);

        //Debug.Log($"tick: {tick}");

        if (visualizeReconciliation)
        {
            serverCube.transform.position = serverState.GetPosition();
            clientCube.transform.position = clientState.GetPosition();
        }

        if (Vector3.Distance(clientState.GetPosition(), serverState.GetPosition()) > NetworkManagement.Instance.ReconcilidationTreshold)
        {
            if (usesNavmeshAgent)
            {
                ResetNavMeshAgent(serverState.GetPosition());

                int reconcileTick = tick;
                while (reconcileTick < NetworkManager.LocalTime.Time)
                {
                    InputState rewindInputState = ServerApprovedInputStates.Get(reconcileTick);

                    agent.SetDestination(rewindInputState.NewDestination);

                    tick++;
                }
            }
            else
            {
                //transform.position = serverState.GetPosition();
                //transform.rotation = serverState.GetQuatRotation();

                //int reconcileTick = tick;
                //while(reconcileTick < NetworkManager.LocalTime.Time)
                //{
                //    InputState rewindInputState = ServerApprovedInputStates.Get(reconcileTick);


                //}
            }
        }

        LastReconciledState = lastServerState;
    }
    void ResetNavMeshAgent(Vector3 warpPosition)
    {
        agent.enabled = false;

        if (NavMesh.SamplePosition(warpPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            //Debug.Log(hit.position);
            agent.Warp(hit.position);
            agent.enabled = true;
        }
    }
    void HandleInterpolation()
    {
        if (IsOwner || IsServer) return;

        TransformState transformState = GetTransformState(
                 NetworkManager.ServerTime.Time -
                 NetworkManagement.Instance.InterpolationDelay / 1000f);

        if (ClientBuffer.Get(NetworkManager.LocalTime.Tick) == ClientBuffer.Get(NetworkManager.LocalTime.Tick - 1)) return;

        TransformState lerpedTS = ServerUpdatesBuffer.Get(transformState);

        transform.position = lerpedTS.GetPosition();
        transform.rotation = lerpedTS.GetQuatRotation();
    }
    TransformState GetTransformState(float delay = 0)
    {
        TransformState ts = new TransformState(NetworkManager.Singleton.LocalTime.Tick, NetworkManager.Singleton.LocalTime.Time - delay, transform.position, transform.rotation.eulerAngles, TransformState.ToSynchVector(posX, posY, posZ), TransformState.ToSynchVector(rotX, rotY, rotZ));
        return ts;
    }
    TransformState GetTransformState(double time)
    {
        return new TransformState(NetworkManager.Singleton.LocalTime.Tick, time, transform.position, transform.rotation.eulerAngles, TransformState.ToSynchVector(posX, posY, posZ), TransformState.ToSynchVector(rotX, rotY, rotZ));
    }
    void GetCubesTransforms()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name.ToLower().Contains("servercube"))
            {
                serverCube = child;
            }
            if (child.gameObject.name.ToLower().Contains("clientcube"))
            {
                clientCube = child;
            }
        }
    }
}