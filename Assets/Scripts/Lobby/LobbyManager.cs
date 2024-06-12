using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : SingeltonPersistant<LobbyManager>
{
    public string PlayerName { get; private set; }
    public Lobby JoinedLobby { get; private set; }

    public override void Awake()
    {
        base.Awake();

        PlayerName = $"Player{Random.Range(0, 100)}";

        Authenticate(PlayerName);
    }

    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
            }
        };
    }

    public async void Authenticate(string playerName)
    {
        try
        {
            this.PlayerName = playerName;

            InitializationOptions options = new();
            options.SetProfile(playerName);

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log($"Signed in! {AuthenticationService.Instance.PlayerId} {AuthenticationService.Instance.PlayerName}");

                EventManager.OnSignedIn.Invoke();
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false, string lobbyPassword = "")
    {
        try
        {
            Debug.Log(lobbyName);
            Debug.Log(lobbyPassword);

            CreateLobbyOptions options = options = new();

            if (isPrivate)
            {
                options.IsPrivate = isPrivate;
                options.Password = lobbyPassword;
            }

            if (lobbyName == string.Empty)
            {
                lobbyName = $"Lobby{Random.Range(0, 10000)}";
            }

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            JoinedLobby = lobby;

            StartCoroutine(HandleLobbyHartBeat());

            Debug.Log($"Created Lobby! {lobby.Name} {lobby.MaxPlayers} {lobby.IsPrivate}");

            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

        return null;
    }
    IEnumerator HandleLobbyHartBeat()
    {
        while (true)
        {
            Task task = new Task(async () =>
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
                Debug.Log("sended");
            });

            task.Start();

            yield return new WaitUntil(() => task.IsCompleted);

            Debug.Log("hearth beat");

            yield return new WaitForSeconds(15);
        }
    }

    public async Task<List<Lobby>> GetLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(options);

            return response.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

        return null;
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
