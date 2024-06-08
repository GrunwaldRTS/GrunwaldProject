using System.Collections;
using System.Collections.Generic;
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

    const string KEY_START_GAME = "StartGame_RelayCode";

    public override void Awake()
    {
        base.Awake();

        PlayerName = $"Player{Random.Range(0, 100)}";

        Authenticate(PlayerName);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public async void Authenticate(string playerName)
    {
        try
        {
            PlayerName = playerName;

            InitializationOptions options = new();
            options.SetProfile(playerName);

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log($"Authenticated client with data: {AuthenticationService.Instance.PlayerId} {AuthenticationService.Instance.PlayerName}");

                EventManager.OnSignedIn.Invoke();
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e.Message);
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
    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false, string lobbyPassword = "")
    {
        try
        {
            CreateLobbyOptions options = options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            if (isPrivate)
            {
                options.IsPrivate = isPrivate;
                options.Password = lobbyPassword;
            }

            if (lobbyName == string.Empty) lobbyName = $"Lobby{Random.Range(0, 10000)}";

            JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            StartCoroutine(HandleLobbyHartBeat());
            StartCoroutine(HandleLobbyPolling());

            Debug.Log($"Created lobby with data: {JoinedLobby.Name} {JoinedLobby.MaxPlayers} {JoinedLobby.IsPrivate}");

            return JoinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }
    void StopCoroutines()
    {
        StopCoroutine(HandleLobbyHartBeat());
        StopCoroutine(HandleLobbyPolling());
    }
    IEnumerator HandleLobbyHartBeat()
    {
        while (true)
        {
            Task task = new Task(async () =>
            {
                if (JoinedLobby is null) return;

                await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
            });

            task.Start();

            yield return new WaitUntil(() => task.IsCompleted);
            yield return new WaitForSeconds(15);
        }
    }
    IEnumerator HandleLobbyPolling()
    {
        while (true)
        {
            Task task = new Task(async () =>
            {
                if (JoinedLobby is null) return;

                JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);

                if (JoinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (IsLobbyHost())
                    {
                        RelayManager.Instance.JoinRelay(JoinedLobby.Data[KEY_START_GAME].Value);
                    }

                    StopCoroutines();
                }
            });

            task.Start();

            yield return new WaitUntil(() => task.IsCompleted);
            yield return new WaitForSeconds(1.1f);
        }
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            Debug.Log($"Joined lobby with lobby code: {lobbyCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public bool IsLobbyHost()
    {
        return JoinedLobby.HostId != AuthenticationService.Instance.PlayerId;
    }
    public async void StartGame()
    {
        try
        {
            if (JoinedLobby is null) return;
            if (!IsLobbyHost()) return;

            string relayCode = await RelayManager.Instance.CreateRelay(JoinedLobby.MaxPlayers);

            UpdateLobbyOptions options = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };

            JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
}