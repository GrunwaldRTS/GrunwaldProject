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
    VisualElement createLobbyModal;
    private void Awake()
    {
        AssignVisualElements();

        LobbyManager manager = LobbyManager.Instance;

        EventManager.OnSignedIn.AddListener(LoadLobbies);

        SubscribeToRefreshLobbies();
        SubscribeToOpenCreateLobbyModal();
        SubscribeToCreateLobbyModalButtons();
    }
    void AssignVisualElements()
    {
        rootEl = lobbyPage.rootVisualElement;
        body = rootEl.Q("body");
        main = body.Q("main");
        createLobbyModal = body.Q("createLobbyModal");
    }
    async void LoadLobbies()
    { 
        List<Lobby> lobbies = await LobbyManager.Instance.GetLobbies();

        VisualElement lobbiesEl = rootEl.Q("lobbies");
        
        foreach(Lobby lobby in lobbies)
        {
            VisualElement lobbieEl = new();
            lobbieEl.AddToClassList("lobby");
            lobbiesEl.Add(lobbieEl);

            Label lobbyLabelEl = new Label(lobby.Name);
            lobbieEl.Add(lobbyLabelEl);
        }
    }
    void SubscribeToRefreshLobbies()
    {
        Button refreshLobbiesButton = main.Q<Button>("refreshLobbies");

        refreshLobbiesButton.clicked += () =>
        {
            DeleteLobbiesEl();

            LoadLobbies();
        };
    }
    void DeleteLobbiesEl()
    {
        VisualElement lobbiesEl = rootEl.Q("lobbies");
        List<VisualElement> lobbyEls = lobbiesEl.Query(className: "lobby").ToList();

        foreach(VisualElement lobbyEl in lobbyEls)
        {
            lobbiesEl.Remove(lobbyEl);
        }
    }
    void SubscribeToOpenCreateLobbyModal()
    {
        Button openCreateLobbyModalButton = main.Q<Button>("openCreateLobbyModalButton");

        openCreateLobbyModalButton.clicked += () => 
        {
            createLobbyModal.style.visibility = Visibility.Visible;
        };
    }
    void SubscribeToCreateLobbyModalButtons()
    {
        Button createLobbyButton = createLobbyModal.Q<Button>("createLobbyButton");
        Button cancelCreateLobbyButton = createLobbyModal.Q<Button>("cancelCreateLobbyButton");

        createLobbyButton.clicked += async () =>
        {
            string lobbyName = createLobbyModal.Q<TextField>("createLobbyName").value;
            string lobbyPassword = createLobbyModal.Q<TextField>("createLobbyPassword").value;
            int lobbyMaxPlayers = createLobbyModal.Q<SliderInt>("createLobbyMaxPlayers").value;

            await LobbyManager.Instance.CreateLobby(lobbyName, lobbyMaxPlayers, lobbyPassword != string.Empty, lobbyPassword);
        };
        cancelCreateLobbyButton.clicked += () =>
        {
            createLobbyModal.style.visibility = Visibility.Hidden;
        };
    }
}
