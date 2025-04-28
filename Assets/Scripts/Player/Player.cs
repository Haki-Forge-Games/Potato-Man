using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float movementSpeed = 50f;
    public float pickUpDistance = 50f;
    public float Sensitivity = 100f;
    public Transform holdPosition;
    public Camera camera;
    public GameObject playerModel;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        if (playerModel == null || !IsOnlineMode) return;

        if (!IsOwner)
        {
            playerModel.layer = 0;
            foreach (Transform child in playerModel.transform)
            {
                child.gameObject.layer = 0;
            }
        }
    }

    [ServerRpc]
    public void ChangeStateServerRpc(NetworkObjectReference enemyRef)
    {
        if (!enemyRef.TryGet(out NetworkObject ntwObject)) return;

        StateManager controller = ntwObject.gameObject.GetComponent<StateManager>();

        // set enemy to death state pn server 
        controller.ChangeStateToDeath();

        // Reflecting changes on client side 
        ChangeStateClientRpc(ntwObject.NetworkObjectId);
    }

    [ClientRpc]
    private void ChangeStateClientRpc(ulong objectId)
    {
        Debug.Log("client rpc");

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject ntwObject))
        {
            Debug.LogError($"NetworkObject with ID {objectId} not found on client.");
            return;
        }

        StateManager controller = ntwObject.gameObject.GetComponent<StateManager>();
        controller.ChangeStateToDeath();
    }

    [ServerRpc]
    public void RemoveBulletFromWorldServerRpc(NetworkObjectReference bulletRef)
    {
        if (!bulletRef.TryGet(out NetworkObject bulletObject)) return;
        bulletObject.Despawn(true);
    }
}
