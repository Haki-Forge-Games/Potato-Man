using TMPro;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    private UiManagerLobby uiManagerLobby;
    private RelayManager relayManager;
    private Logger logger;

    private void Start()
    {
        Initialize();
        ValidateReferences();
    }

    private void Initialize()
    {
        uiManagerLobby = UiManagerLobby.Instance;
        relayManager = RelayManager.Instance;
        logger = Logger.Instance;
    }


    public async void CreateLobby()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        if (uiManagerLobby == null || relayManager == null)
        {
            uiManagerLobby.ShowFeedback("Something went wrong");
            return;
        }

        if (string.IsNullOrEmpty(uiManagerLobby.lobbyNameInput?.text) || string.IsNullOrEmpty(uiManagerLobby.lobbyPasswordInput?.text))
        {
            uiManagerLobby.ShowFeedback("All fields are required");
            return;
        }

        try
        {
            uiManagerLobby.waitingScreen?.SetActive(true);

            var (lobbyCount, existedLobby) = await FindLobbyByName(uiManagerLobby.lobbyNameInput.text);

            if (lobbyCount > 0)
            {
                uiManagerLobby.ShowFeedback("Lobby with same name already exists");
                uiManagerLobby.waitingScreen?.SetActive(false);
                return;
            }

            // creating allocation 
            var (joinCode, allocation) = await relayManager.CreateAllocation(2);

            if (string.IsNullOrEmpty(joinCode) || allocation == null)
            {
                uiManagerLobby.ShowFeedback("Something went wrong");
                return;
            }

            CreateLobbyOptions options = new CreateLobbyOptions();

            // setting data 
            options.Data = new Dictionary<string, DataObject>
                {
                    { "Password", new DataObject(DataObject.VisibilityOptions.Public, uiManagerLobby.lobbyPasswordInput?.text) },
                    { "AllocationCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(uiManagerLobby.lobbyNameInput?.text, 2, options);
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            uiManagerLobby.waitingScreen?.SetActive(false);
            uiManagerLobby.CloseCreateRoom();

            // seting relay transport data 
            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);



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

        if (uiManagerLobby == null || relayManager == null)
        {
            uiManagerLobby.ShowFeedback("Something went wrong");
            return;
        }

        if (string.IsNullOrEmpty(uiManagerLobby.joinLobbyNameInput?.text) || string.IsNullOrEmpty(uiManagerLobby.joinLobbyPasswordInput?.text))
        {
            uiManagerLobby.ShowFeedback("All fields are required to join a lobby");
            return;
        }

        try
        {
            uiManagerLobby.waitingScreen?.SetActive(true);

            // finding lobby by named 
            var (lobbyCount, lobby) = await FindLobbyByName(uiManagerLobby.joinLobbyNameInput.text);

            if (lobbyCount == 0)
            {
                uiManagerLobby.ShowFeedback("Lobbyname or Password is incorrect");
                uiManagerLobby.waitingScreen?.SetActive(false);
                return;
            }

            // checking if password is correct or not 
            if (lobby.Data["Password"].Value == uiManagerLobby.joinLobbyPasswordInput.text)
            {

                // joining lobby 
                Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

                // creating allocation 
                var joinAllocation = await relayManager.JoinRelayAllocation(joinedLobby.Data["AllocationCode"]?.Value);

                if (joinAllocation == null)
                {
                    uiManagerLobby.ShowFeedback("Something went wrong");
                    return;
                }

                // seting relay transport data 
                var relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                uiManagerLobby.ShowFeedback($"Lobby found {joinedLobby.Id}", true);
                uiManagerLobby.HandleScreensToggles(uiManagerLobby?.joinRoomScreen);

                // starting host 
                NetworkManager.Singleton.StartClient();
            }
            else
            {
                uiManagerLobby.ShowFeedback("Lobbyname or Password is incorrect");
            }

            uiManagerLobby.waitingScreen?.SetActive(false);

        }
        catch (Exception ex)
        {
            uiManagerLobby.ShowFeedback($"Failed to join lobby {ex.Message}");
            uiManagerLobby.waitingScreen?.SetActive(false);
        }
    }

    // find lobby by name 
    private async Task<(int, Lobby)> FindLobbyByName(string lobbyName)
    {

        if (string.IsNullOrEmpty(lobbyName))
        {
            return (0, null);
        }

        try
        {
            // creating filters to find lobby 
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.Name,
                    op: QueryFilter.OpOptions.EQ,
                    value: lobbyName
                )
            };

            // finding lobby 
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            return lobbies.Results.Count == 0 ? (0, null) : (lobbies.Results.Count, lobbies.Results[0]);

        }
        catch (Exception ex)
        {
            return (0, null);
        }
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds = 15)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (uiManagerLobby == null) logger.LoggerError("[LobbyManager] UiManagerLobby is not assigned.");
        if (relayManager == null) logger.LoggerError("[LobbyManager] relayManager is not assigned.");
    }

}