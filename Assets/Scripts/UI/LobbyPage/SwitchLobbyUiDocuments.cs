using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwitchLobbyUiDocuments : MonoBehaviour
{
    [SerializeField] LobbyPage lobbyPage;
    [SerializeField] LobbiesPage lobbiesPage;

    private void Awake()
    {
        EventManager.OnJoinedLobby.AddListener(EnableLobbyPage);
        EventManager.OnLeftLobby.AddListener(EnableLobbiesPage);
        EventManager.OnChunkMeshesInstanced.AddListener(DisableLobbyUI);
    }

    void EnableLobbiesPage()
    {
        Debug.Log("Enable lobbies page");
        lobbyPage.DisablePage();
        lobbiesPage.EnablePage();
    }

    void EnableLobbyPage()
    {
        Debug.Log("Enable lobby page");
        lobbiesPage.DisablePage();
        lobbyPage.EnablePage();
    }

    void DisableLobbyUI()
    {
        Debug.Log("Disable lobby UI");
        lobbiesPage.DisablePage();
        lobbyPage.DisablePage();
    }
    private void OnDestroy()
    {
        EventManager.OnJoinedLobby.RemoveListener(EnableLobbyPage);
        EventManager.OnLeftLobby.RemoveListener(EnableLobbiesPage);
        EventManager.OnChunkMeshesInstanced.RemoveListener(DisableLobbyUI);
    }
}
