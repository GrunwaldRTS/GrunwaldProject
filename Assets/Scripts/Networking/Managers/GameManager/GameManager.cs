using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    public NetworkList<ulong> PlayerIds { get; private set; } = new();
    public NetworkList<PlayerData> PlayersData { get; private set; } = new();

    private void Awake()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("OnNetworkSpawn");

        base.OnNetworkSpawn();

        SpawnPlayerWithId(NetworkObject.OwnerClientId);

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerWithId;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        base.OnNetworkDespawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerWithId;
    }
    public void SpawnPlayerWithId(ulong id)
    {
        Debug.Log("Spawn Player");

        if (!IsServer) return;
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);

        PlayerIds.Add(id);
        PlayersData.Add(new(100, 100, 100));
    }
}
