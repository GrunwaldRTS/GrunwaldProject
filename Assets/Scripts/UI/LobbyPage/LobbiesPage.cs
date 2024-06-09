using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbiesPage : MonoBehaviour
{
    [SerializeField] UIDocument lobbyPage;
    VisualElement rootEl;
    VisualElement body;
    VisualElement main;
    VisualElement createLobbyModal;
    VisualElement directJoinLobbyModal;
    private void Awake()
    {
        AssignVisualElements();

        LobbyManager manager = LobbyManager.Instance;

        EventManager.OnJoinedLobby.AddListener(DisablePage);
        EventManager.OnLeftLobby.AddListener(EnablePage);

        EventManager.OnSignedIn.AddListener(LoadLobbies);

        SubscribeToOpenButtons();
        SubscribeToCreateLobbyModalButtons();
        SubscribeToDirectJoinLobbyModal();
    }
    private void Start()
    {
        EnablePage();
    }
    void EnablePage()
    {
        Debug.Log("Enable Lobbies page");
        StartCoroutine(RefreshLobbies());

        rootEl.style.display = DisplayStyle.Flex;
    }
    void DisablePage()
    {
        Debug.Log("Disable Lobbies page");
        StopAllCoroutines();

        rootEl.style.display = DisplayStyle.None;
    }
    void AssignVisualElements()
    {
        rootEl = lobbyPage.rootVisualElement;
        body = rootEl.Q("body");
        main = body.Q("main");
        createLobbyModal = body.Q("createLobbyModal");
        directJoinLobbyModal = body.Q("directJoinLobbyModal");
    }
    void LoadLobbies()
    {
        //Debug.Log("Update Lobbies Data");

        List<Lobby> lobbies = LobbyManager.Instance.PublicLobbies;

        if (lobbies is null) return;

        VisualElement lobbiesEl = rootEl.Q("lobbies");

        foreach (Lobby lobby in lobbies)
        {
            VisualElement lobbieEl = new();
            lobbieEl.AddToClassList("lobby");
            lobbiesEl.Add(lobbieEl);

            Label lobbyLabelEl = new Label(lobby.Name);
            lobbieEl.Add(lobbyLabelEl);

            Button joinLobbyButton = new();
            joinLobbyButton.AddToClassList("btn");
            joinLobbyButton.text = "Join Lobby";
            joinLobbyButton.clicked += () =>
            {
                LobbyManager.Instance.JoinLobbyById(lobby.Id);
            };
            lobbieEl.Add(joinLobbyButton);
        }
    }
    IEnumerator RefreshLobbies()
    {
        while (true)
        {
            DeleteLobbiesEl();

            LoadLobbies();

            yield return new WaitForSeconds(1.1f);
        }
    }
    void DeleteLobbiesEl()
    {
        VisualElement lobbiesEl = rootEl.Q("lobbies");
        List<VisualElement> lobbyEls = lobbiesEl.Query(className: "lobby").ToList();

        foreach (VisualElement lobbyEl in lobbyEls)
        {
            lobbiesEl.Remove(lobbyEl);
        }
    }
    void SubscribeToOpenButtons()
    {
        Button openCreateLobbyModalButton = main.Q<Button>("openCreateLobbyModalButton");
        Button openDirectJoinModalButton = main.Q<Button>("openDirectJoinModalButton");

        openCreateLobbyModalButton.clicked += () =>
        {
            createLobbyModal.style.display = DisplayStyle.Flex;
        };
        openDirectJoinModalButton.clicked += () =>
        {
            directJoinLobbyModal.style.display = DisplayStyle.Flex;
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

            createLobbyModal.style.display = DisplayStyle.None;
        };
        cancelCreateLobbyButton.clicked += () =>
        {
            createLobbyModal.style.display = DisplayStyle.None;
        };
    }
    void SubscribeToDirectJoinLobbyModal()
    {
        Button directJoinLobbyButton = directJoinLobbyModal.Q<Button>("directJoinLobbyButton");
        Button cancelDirectJoinLobbyButton = directJoinLobbyModal.Q<Button>("cancelDirectJoinLobbyButton");

        directJoinLobbyButton.clicked += () =>
        {
            string directJoinLobbyCode = directJoinLobbyModal.Q<TextField>("directJoinLobbyCode").value;
            LobbyManager.Instance.JoinLobbyByCode(directJoinLobbyCode);
            directJoinLobbyModal.style.display = DisplayStyle.None;
        };
        cancelDirectJoinLobbyButton.clicked += () =>
        {
            directJoinLobbyModal.style.display = DisplayStyle.None;
        };
    }
}
