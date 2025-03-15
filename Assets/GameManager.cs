using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    private UiManagerLobby uiManager;
    private Logger logger;

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

    private void Start()
    {
        Initialize();
        SubscribeMethods();
    }

    private void Initialize()
    {
        uiManager = UiManagerLobby.Instance;
        logger = Logger.Instance;
    }

    private void SubscribeMethods()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        }
    }

    private void ClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {

        }
    }

    private void StartLobbyGame()
    {

    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (uiManager == null) logger.LoggerError("[GameManager] uiManager is not assigned.");
    }
}
