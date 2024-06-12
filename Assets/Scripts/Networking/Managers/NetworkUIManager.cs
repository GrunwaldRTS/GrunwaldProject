using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField][Range(0, 1)] float width = 0.2f;
    [SerializeField][Range(0, 1)] float height = 0.2f;
    private void OnGUI()
    {
        float w = Screen.width * width;
        float h = Screen.height * height;
        GUILayout.BeginArea(new Rect(10, 10, w, h));

        NetworkManager netManager = NetworkManager.Singleton;
        if (!netManager.IsClient && !netManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabel();
            SubmitNewPosition();
        }
        GUILayout.EndArea();
    }
    void StartButtons()
    {
        if (GUILayout.Button("Server"))
        {
            NetworkManager.Singleton.StartServer();
        }
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
    void StatusLabel()
    {
        NetworkManager netManager = NetworkManager.Singleton;
        string status = netManager.IsServer ? "Server" : netManager.IsHost ? "Host" : "Client";
        GUILayout.Label(status);
    }
    void SubmitNewPosition()
    {
        NetworkManager netManager = NetworkManager.Singleton;

        string text = netManager.IsServer ? "Move" : "Request movement";
        if (GUILayout.Button(text))
        {
            if (netManager.IsServer && !netManager.IsClient)
            {
                foreach (ulong uid in netManager.ConnectedClientsIds)
                {
                    netManager.SpawnManager.GetPlayerNetworkObject(uid);
                }
            }
            else
            {
                var playerObject = netManager.SpawnManager.GetLocalPlayerObject();

            }
        }
    }
}
