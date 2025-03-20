using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawningManagerLobby : MonoBehaviour
{

    [Header("Player Prefab")]
    public GameObject player;

    [Header("Player Positions")]
    public List<GameObject> playerPositions;

    private Logger logger;
    public static SpawningManagerLobby Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Initialize();
        SubscribeMethods();
        ValidateReferences();
    }

    private void Initialize()
    {
        logger = Logger.Instance;
    }

    private void SubscribeMethods()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    // runs every time when a client is connected 
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // finding client index 
            List<ulong> clientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
            int playerIndex = clientIds.IndexOf(clientId);

            SpawnPlayerServerRpc(clientId, playerIndex);
        }
    }

    public void SpawnPlayerServerRpc(ulong clientId, int playerIndex = 0)
    {
        if (player == null) return;
        GameObject playerInstance = Instantiate(player);
        playerInstance.transform.position = playerPositions[playerIndex].transform.localPosition;
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (player == null) logger.LoggerError("[SpawningManagerLobby] player is not assigned.");
        if (playerPositions.Count == 0) logger.LoggerError("[SpawningManagerLobby] playerPositions are not assigned.");
    }
}
