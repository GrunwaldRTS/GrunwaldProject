using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    Lobby hostLobby;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    IEnumerator HandleLobbyHartBeat()
    {
        while (true)
        {
            Task task = new Task(async () =>
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            });

            yield return new WaitUntil(() => task.IsCompleted );

            yield return new WaitForSeconds(15);
        }
    }

    async void CreateLobby()
    {
        string lobbyName = "MyLobby";
        int maxPlayers = 4;

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            StartCoroutine(HandleLobbyHartBeat());

            Debug.Log($"Created Lobby! {lobby.Name} {lobby.MaxPlayers}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    async void ListLobbies()
    {
        try
        {
            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
            
            Debug.Log($"Lobbies founc: {response.Results.Count}");
            foreach(Lobby lobby in response.Results)
            {
                Debug.Log($"{lobby.Name} {lobby.MaxPlayers}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    async void JoinLobby()
    {
        await Lobbies.Instance.JoinLobbyByIdAsync(hostLobby.Id);
    }
}
