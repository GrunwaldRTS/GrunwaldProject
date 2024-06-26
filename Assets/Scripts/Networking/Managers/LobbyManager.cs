using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : Singelton<LobbyManager>
{
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public List<Lobby> PublicLobbies { get; set; }
    public Lobby JoinedLobby { get; private set; }

    const string KEY_START_GAME = "StartGame_RelayCode";

    private void Awake()
    {
        PlayerName = $"Player{Random.Range(0, 100)}";

        Authenticate(PlayerName);
        
    }
    private void Start()
    {
        StartCoroutine(HandleLobbiesPolling());
    }
    public async void Authenticate(string playerName)
    {
        try
        {
            PlayerName = playerName;

            InitializationOptions options = new();
            options.SetProfile(playerName);

            await UnityServices.InitializeAsync(options);

            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log($"Authenticated client with data: {AuthenticationService.Instance.PlayerId} {AuthenticationService.Instance.PlayerName}");

                PlayerId = AuthenticationService.Instance.PlayerId;

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
            CreateLobbyOptions options = options = new CreateLobbyOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            //if (isPrivate)
            //{
            //    options.IsPrivate = isPrivate;
            //    options.Password = lobbyPassword;
            //}
            options.IsPrivate = false;

            if (lobbyName == string.Empty) lobbyName = $"Lobby{Random.Range(0, 10000)}";

            JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log(JoinedLobby.HostId);
            Debug.Log($"Created lobby with data: {JoinedLobby.Name} {JoinedLobby.MaxPlayers} {JoinedLobby.IsPrivate} {JoinedLobby.LobbyCode}");
            EventManager.OnJoinedLobby.Invoke();

            StartCoroutine(HandleLobbyHartBeat());

            return JoinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
            return null;
        }
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
    bool deletedLobby;
    IEnumerator HandleLobbiesPolling()
    {
        while (true)
        {
            Task task = new (async () =>
            {
                try
                {
                    PublicLobbies = await GetLobbies();

                    if (JoinedLobby is null) return;

                    JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    if (e.ApiError.Code == 16001)
                    {
                        Debug.Log("Lobby deleted");
                        deletedLobby = true;
                        return;
                    }

                    Debug.LogError(e);
                }
            });

            task.Start();

            yield return new WaitUntil(() => task.IsCompleted);

            if (deletedLobby)
            {
                deletedLobby = false;
                EventManager.OnLeftLobby.Invoke();
                JoinedLobby = null;
            }

            if (JoinedLobby is not null)
            {
                if (!JoinedLobby.Players.Select(player => player.Id).Contains(PlayerId))
                {
                    Debug.Log("No longer in lobby");
                    EventManager.OnLeftLobby.Invoke();
                    JoinedLobby = null;
                }

                if (JoinedLobby is not null)
                {
                    if (JoinedLobby.Data is not null)
                    {
                        if (JoinedLobby.Data[KEY_START_GAME].Value != "0")
                        {
                            if (!IsLobbyHost())
                            {
                                Debug.Log("Started Game");
                                SceneManager.LoadScene("TestProceduralGenerationScene");
                                RelayManager.Instance.JoinRelay(JoinedLobby.Data[KEY_START_GAME].Value);
                            }

                            StopAllCoroutines();
                        }
                    }
                }
           }

            yield return new WaitForSeconds(1.5f);
        }
    }
    private async Task<List<Lobby>> GetLobbies()
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
    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                    }
                }
            };

            JoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log($"Joined lobby with lobby id: {lobbyId}");

            EventManager.OnJoinedLobby.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                    }
                }
            };

            JoinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log($"Joined lobby with lobby code: {lobbyCode}");

            EventManager.OnJoinedLobby.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public async void LeaveLobby()
    {
        try
        {
            if (JoinedLobby is null) return;

            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

            Debug.Log($"Left lobby with id: {JoinedLobby.Id}");

            JoinedLobby = null;

            EventManager.OnLeftLobby.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public async void KickPlayerById(string playerId)
    {
        try
        {
            if (JoinedLobby is null) return;
            if (!IsLobbyHost()) return;

            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, playerId);

            Debug.Log($"Kicked player with id: {playerId}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public async void DeleteLobbyById(string lobbyId)
    {
        try
        {
            if(!await IsLobbyHost(lobbyId)) return;

            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);

            Debug.Log($"Deleted lobby with id: {lobbyId}");

            JoinedLobby = null;

            EventManager.OnLeftLobby.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public bool IsLobbyHost()
    {
        return JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public async Task<bool> IsLobbyHost(string lobbyId)
    {
        Lobby lobbyToDelete = await LobbyService.Instance.GetLobbyAsync(lobbyId);

        return lobbyToDelete.HostId == AuthenticationService.Instance.PlayerId;
    }
    public IEnumerator StartGame()
    {
        if (JoinedLobby is null) yield break;
        if (!IsLobbyHost()) yield break;

        AsyncOperation loadSceneTask = SceneManager.LoadSceneAsync("TestProceduralGenerationScene", LoadSceneMode.Additive);
        while (!loadSceneTask.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestProceduralGenerationScene"));

        yield return null;

        StartRelay();
    }
    async void StartRelay()
    {
        string relayCode = await RelayManager.Instance.CreateRelay(JoinedLobby.MaxPlayers - 1);

        UpdateLobbyOptions options = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new(DataObject.VisibilityOptions.Member, relayCode) }
                }
        };

        JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, options);

        Debug.Log($"Started Game with relay code: {relayCode}");
    }
}