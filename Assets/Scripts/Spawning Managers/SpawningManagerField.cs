using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawningManagerField : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    private GameManager gameManager;
    private Logger logger;

    private void Start()
    {
        Initialize();
        HandleMultiplayerSpawning();
        ValidateReferences();
    }

    private void Initialize()
    {
        gameManager = GameManager.Instance;
        logger = Logger.Instance;
    }

    private void HandleMultiplayerSpawning()
    {
        if (gameManager == null || !gameManager.isMultiplayer) return;

        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            StartCoroutine(SpawnPlayerWithDelay(client.ClientId));

        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null) return;

        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private IEnumerator SpawnPlayerWithDelay(ulong clientId)
    {
        yield return new WaitForSeconds(1.5f);
        SpawnPlayer(clientId);
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (playerPrefab == null) logger.LoggerError("[SpawningManagerField] playerPrefab is not assigned.");
        if (gameManager == null) logger.LoggerError("[SpawningManagerField] gameManager is not assigned.");
    }
}
