using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LobbyManager : MonoBehaviour
{
    private UiManagerLobby uiManagerLobby;
    private Logger logger;

    private void Start()
    {
        Initialize();
        ValidateReferences();
    }

    private void Initialize()
    {
        uiManagerLobby = UiManagerLobby.Instance;
        logger = Logger.Instance;
    }


    public async void CreateLobby()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        if (uiManagerLobby == null) return;

        if (string.IsNullOrEmpty(uiManagerLobby.lobbyNameInput?.text) || string.IsNullOrEmpty(uiManagerLobby.lobbyPasswordInput?.text))
        {
            uiManagerLobby.ShowFeedback("All fields are required");
            return;
        }

        try
        {
            uiManagerLobby.waitingScreen?.SetActive(true);
            CreateLobbyOptions options = new CreateLobbyOptions();

            // setting data 
            options.Data = new Dictionary<string, DataObject>
                {
                    { "Password", new DataObject(DataObject.VisibilityOptions.Member, uiManagerLobby.lobbyPasswordInput?.text) },
                };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(uiManagerLobby.lobbyNameInput?.text, 2, options);

            uiManagerLobby.waitingScreen?.SetActive(false);
            uiManagerLobby.CloseCreateRoom();

            // starting host 
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception ex)
        {
            uiManagerLobby.ShowFeedback($"Failed to create lobby {ex.Message}");
            uiManagerLobby.waitingScreen?.SetActive(false);
        }
    }


    public async void JoinLobby()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        if (uiManagerLobby == null) return;

        if (string.IsNullOrEmpty(uiManagerLobby.joinLobbyNameInput?.text) || string.IsNullOrEmpty(uiManagerLobby.joinLobbyPasswordInput?.text))
        {
            uiManagerLobby.ShowFeedback("All fields are required to join a lobby");
            return;
        }

        try
        {
            uiManagerLobby.waitingScreen?.SetActive(true);
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.Name,
                    op: QueryFilter.OpOptions.EQ,
                    value: uiManagerLobby.joinLobbyNameInput.text)
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            if (lobbies.Results.Count == 0)
            {
                uiManagerLobby.ShowFeedback("No lobbies exists with this name");
                uiManagerLobby.waitingScreen?.SetActive(false);
                return;
            }

            // starting host 
            uiManagerLobby.waitingScreen?.SetActive(false);
            uiManagerLobby.ShowFeedback($"Lobby found {lobbies.Results[0].Id}", true);
            uiManagerLobby.HandleScreensToggles(uiManagerLobby?.joinRoomScreen);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception ex)
        {
            uiManagerLobby.ShowFeedback($"Failed to join lobby {ex.Message}");
            uiManagerLobby.waitingScreen?.SetActive(false);
        }
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (uiManagerLobby == null) logger.LoggerError("[LobbyManager] UiManagerLobby is not assigned.");
    }

}





