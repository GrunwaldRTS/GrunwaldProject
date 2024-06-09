using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPage : MonoBehaviour
{
    [SerializeField] UIDocument lobbyPage;
    VisualElement rootEl;
    VisualElement body;
    VisualElement main;
    VisualElement playersEl;
    VisualElement dataEl;
    private void Awake()
    {
        AssignVisualElements();

        EventManager.OnJoinedLobby.AddListener(EnablePage);
        EventManager.OnLeftLobby.AddListener(DisablePage);

        SubscribeToLobbyButtons();
    }
    private void Start()
    {
        DisablePage();
    }
    void EnablePage()
    {
        Debug.Log("Enable Lobby page");
        StartCoroutine(RefreshLobby());

        rootEl.style.display = DisplayStyle.Flex;
    }
    void DisablePage()
    {
        Debug.Log("Disable Lobby page");
        StopAllCoroutines();

        rootEl.style.display = DisplayStyle.None;
    }
    void AssignVisualElements()
    {
        rootEl = lobbyPage.rootVisualElement;
        body = rootEl.Q("body");
        main = body.Q("main");
        playersEl = main.Q("players");
        dataEl = main.Q("data");
    }
    IEnumerator RefreshLobby()
    {
        while (true)
        {
            DeletePlayers();

            LoadLobbyData();

            yield return new WaitForSeconds(1.1f);
        }
    }
    void DeletePlayers()
    {
        List<VisualElement> playerEls = playersEl.Query("player").ToList();

        foreach (VisualElement playerEl in playerEls)
        {
            playersEl.Remove(playerEl);
        }
    }
    void SubscribeToLobbyButtons()
    {
        Button startGameButton = main.Q<Button>("startGameButton");
        Button leaveLobbyButton = main.Q<Button>("leaveLobbyButton");
        Button deleteLobbyButton = main.Q<Button>("deleteLobbyButton");

        startGameButton.clicked += () =>
        {
            LobbyManager.Instance.StartGame();
        };
        leaveLobbyButton.clicked += () =>
        {
            LobbyManager.Instance.LeaveLobby();
        };
        deleteLobbyButton.clicked += () =>
        {
            LobbyManager.Instance.DeleteLobbyById(LobbyManager.Instance.JoinedLobby.Id);
        };
    }
    void LoadLobbyData()
    {
        //Debug.Log("Update Lobby Data");
        Lobby lobby = LobbyManager.Instance.JoinedLobby;

        if (lobby is null) return;

        List<Player> players = lobby.Players;

        if (players is null) return;

        int playersCount = players.Count;
        int maxPlayers = lobby.MaxPlayers;

        Label lobbyName = main.Q<Label>("lobbyName");
        lobbyName.text = $"Lobby: {lobby.Name}";

        Label playerCountLabel = dataEl.Q<Label>("playersCount");

        playerCountLabel.text = $"{playersCount}/{maxPlayers}";

        foreach (Player player in players)
        {
            if (player is null) return;

            VisualElement playerEl = new();
            playerEl.AddToClassList("player");
            playerEl.name = "player";
            playersEl.Add(playerEl);

            if (player.Data is null) return;
            if (player.Data["playerName"].Value is null) return;

            Label playerNameLabel = new(player.Data["playerName"].Value);
            playerNameLabel.AddToClassList("playerName");
            playerEl.Add(playerNameLabel);

            if (!LobbyManager.Instance.IsLobbyHost()) continue;

            Button kickPlayerButton = new();
            kickPlayerButton.AddToClassList("btn");
            kickPlayerButton.text = "Kick player";
            playerEl.Add(kickPlayerButton);

            kickPlayerButton.clicked += () =>
            {
                LobbyManager.Instance.KickPlayerById(player.Id);
            };
        }
    }
}
