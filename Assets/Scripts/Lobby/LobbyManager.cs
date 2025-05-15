// Unity
using UnityEngine;

//  System 
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

// Networking
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    #region Singleton
    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Fields
    private UiManagerLobby uiManagerLobby;
    private GameManager gameManager;
    private RelayManager relayManager;
    private Logger logger;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        Initialize();
        ValidateReferences();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        uiManagerLobby = UiManagerLobby.Instance;
        gameManager = GameManager.Instance;
        relayManager = RelayManager.Instance;
        logger = Logger.Instance;
    }
    #endregion

    #region Lobby Management
    public async void CreateLobby()
    {
        if (!IsAuthenticated() || !AreDependenciesValid()) return;

        string lobbyName = uiManagerLobby.lobbyNameInput?.text;
        string lobbyPassword = uiManagerLobby.lobbyPasswordInput?.text;

        if (string.IsNullOrEmpty(lobbyName) || string.IsNullOrEmpty(lobbyPassword))
        {
            uiManagerLobby.ShowFeedback("All fields are required");
            return;
        }

        uiManagerLobby.HandleScreensToggles(uiManagerLobby.waitingScreen);

        if (await LobbyExists(lobbyName))
        {
            ShowErrorAndReset("Lobby with same name already exists");
            return;
        }

        var (joinCode, allocation) = await relayManager.CreateAllocation(2);
        if (string.IsNullOrEmpty(joinCode) || allocation == null)
        {
            ShowErrorAndReset("Something went wrong");
            return;
        }

        var lobby = await CreateLobbyAsync(lobbyName, lobbyPassword, joinCode);
        if (lobby == null)
        {
            ShowErrorAndReset("Failed to create lobby");
            return;
        }

        gameManager.lobby = lobby;
        StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id));
        ConfigureAndStartHost(allocation);
    }

    public async void JoinLobby()
    {
        if (!IsAuthenticated() || !AreDependenciesValid()) return;

        string lobbyName = uiManagerLobby.joinLobbyNameInput?.text;
        string lobbyPassword = uiManagerLobby.joinLobbyPasswordInput?.text;

        if (string.IsNullOrEmpty(lobbyName) || string.IsNullOrEmpty(lobbyPassword))
        {
            uiManagerLobby.ShowFeedback("All fields are required to join a lobby");
            return;
        }

        uiManagerLobby.HandleScreensToggles(uiManagerLobby.waitingScreen);

        var (_, lobby) = await FindLobbyByName(lobbyName);
        if (lobby == null || lobby.Data["Password"].Value != lobbyPassword)
        {
            ShowErrorAndReset("Lobby name or Password is incorrect");
            return;
        }

        var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
        gameManager.lobby = joinedLobby;

        var joinAllocation = await relayManager.JoinRelayAllocation(joinedLobby.Data["AllocationCode"]?.Value);
        if (joinAllocation == null)
        {
            ShowErrorAndReset("Something went wrong");
            return;
        }

        ConfigureAndStartClient(joinAllocation);
        uiManagerLobby.ShowFeedback($"Lobby found {joinedLobby.Id}", true);
    }
    #endregion

    #region Lobby Helpers
    private async Task<Lobby> CreateLobbyAsync(string name, string password, string joinCode)
    {
        try
        {
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "Password", new DataObject(DataObject.VisibilityOptions.Public, password) },
                    { "AllocationCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };
            return await LobbyService.Instance.CreateLobbyAsync(name, 2, options);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> LobbyExists(string lobbyName)
    {
        var (count, _) = await FindLobbyByName(lobbyName);
        return count > 0;
    }

    private async Task<(int, Lobby)> FindLobbyByName(string lobbyName)
    {
        if (string.IsNullOrEmpty(lobbyName)) return (0, null);
        try
        {
            var options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter> {
                    new QueryFilter(
                    field: QueryFilter.FieldOptions.Name,
                    op: QueryFilter.OpOptions.EQ,
                    value: lobbyName)
                    }
            };
            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            return (lobbies.Results.Count, lobbies.Results.Count > 0 ? lobbies.Results[0] : null);
        }
        catch
        {
            return (0, null);
        }
    }
    #endregion

    #region Network Configuration
    private void ConfigureAndStartHost(Allocation allocation)
    {
        var relayData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
        gameManager.isHost = true;
        gameManager.isMultiplayer = true;

        uiManagerLobby.HandleScreensToggles(uiManagerLobby.waitingScreen);
        uiManagerLobby.HandleScreensToggles(uiManagerLobby.createRoomScreen);

        NetworkManager.Singleton.StartHost();
    }

    private void ConfigureAndStartClient(JoinAllocation joinAllocation)
    {
        var relayData = new RelayServerData(joinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
        gameManager.isHost = false;
        gameManager.isMultiplayer = true;

        uiManagerLobby.HandleScreensToggles(uiManagerLobby.waitingScreen);
        uiManagerLobby.HandleScreensToggles(uiManagerLobby.joinRoomScreen);

        NetworkManager.Singleton.StartClient();
    }
    #endregion

    #region Utility Methods
    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float interval = 15f)
    {
        var delay = new WaitForSecondsRealtime(interval);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private bool IsAuthenticated() => AuthenticationService.Instance.IsSignedIn;
    private bool AreDependenciesValid() => uiManagerLobby != null && relayManager != null && gameManager != null;
    private void ShowErrorAndReset(string message)
    {
        uiManagerLobby.ShowFeedback(message);
        uiManagerLobby.HandleScreensToggles(uiManagerLobby.waitingScreen);
    }

    private void ValidateReferences()
    {
        if (logger == null) return;
        if (uiManagerLobby == null) logger.LoggerError("[LobbyManager] UiManagerLobby is not assigned.");
        if (relayManager == null) logger.LoggerError("[LobbyManager] relayManager is not assigned.");
        if (gameManager == null) logger.LoggerError("[LobbyManager] gameManager is not assigned.");
    }
    #endregion
}
