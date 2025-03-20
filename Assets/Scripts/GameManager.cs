// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

// System
using System.Collections;

// Ui
using TMPro;

// Networking
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

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
    private UiManagerLobby uiManager;
    private Logger logger;

    // Multiplayer Data 
    public bool isHost { get; set; }
    public Lobby lobby { get; set; }

    // Game Mode 
    public bool isMultiplayer { get; set; } = false;
    #endregion

    #region Unity Methods
    private void Start()
    {
        Initialize();
        SubscribeMethods();
        ValidateReferences();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        uiManager = UiManagerLobby.Instance;
        logger = Logger.Instance;
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (uiManager == null) logger.LoggerError("[GameManager] uiManager is not assigned.");
    }
    #endregion

    #region Network Handling
    private void SubscribeMethods()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        }
    }

    private void ClientConnected(ulong clientId)
    {
        if (!IsHostValid()) return;

        ShowOnlinePlayButton();

        if (lobby.MaxPlayers == NetworkManager.Singleton.ConnectedClients.Count)
        {
            EnableOnlinePlayButton();
        }
    }

    private bool IsHostValid()
    {
        return isHost != null && isHost && lobby != null;
    }
    #endregion

    #region UI Management
    private void ShowOnlinePlayButton()
    {
        if (uiManager == null || uiManager.startOnlineGameButton == null) return;
        uiManager.startOnlineGameButton.gameObject.SetActive(true);
    }

    private void EnableOnlinePlayButton()
    {
        if (uiManager == null || uiManager.startOnlineGameButton == null) return;
        uiManager.startOnlineGameButton.interactable = true;
    }
    #endregion

    #region Scene Management
    public void StartLobbyGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true);
            }
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    public IEnumerator LoadSceneAsync(int sceneIndex, TextMeshProUGUI loadingText)
    {
        if (sceneIndex == null || loadingText == null) yield break;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadingText.text = $"{(asyncLoad.progress / 0.9f) * 100f}%";

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    #endregion
}
