using UnityEngine;
using Unity.Netcode;

public class PickUpAndDrop : NetworkBehaviour
{
    #region Variables
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ItemDatabase networkPrefabsList;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    private RaycastHit hit;
    private GameObject heldItem;
    private bool isPickedUp = false;
    #endregion

    #region Update Loop
    private void Update()
    {
        if (inputs.CheckPickUpPressed())
        {
            if (CanPickUpItem())
                TryPickUpItem();
            else if (CanDropItem())
                TryDropItem();
        }
    }
    #endregion

    #region Pickup Logic
    /// <summary>
    /// Checks if the player can pick up an item.
    /// </summary>
    private bool CanPickUpItem()
    {
        return player != null && playerCamera != null &&
               Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, player.pickUpDistance) &&
               hit.collider.CompareTag("pickAbles") && !isPickedUp;
    }

    /// <summary>
    /// Attempts to pick up an item (handles both online and offline modes).
    /// </summary>
    private void TryPickUpItem()
    {
        if (IsOnlineMode)
        {
            if (!IsOwner) return;

            NetworkObject networkObject = hit.collider.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                PickUpServerRpc(networkObject);
            }
        }
        else
        {
            GameObject item = hit.collider.gameObject;
            if (item != null)
            {
                PickUpItem(item);
            }
        }
    }

    /// <summary>
    /// Picks up an item and attaches it to the player.
    /// </summary>
    public void PickUpItem(GameObject itemObject)
    {
        if (itemObject == null) return;

        Item itemComponent = itemObject.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        SpawnItem(itemComponent.afterSpawnPrefab);
        Destroy(itemObject);
        ChangePickUpStatus(true);
    }

    [ServerRpc]
    public void PickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNtwObject)) return;

        Item itemComponent = itemNtwObject.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        SpawnItemClientRpc(itemNtwObject.NetworkObjectId); // Spawn item in the owner client

        // Despawns the world object
        itemNtwObject.Despawn();
    }
    #endregion

    #region Drop Logic
    /// <summary>
    /// Checks if the player can drop the currently held item.
    /// </summary>
    private bool CanDropItem()
    {
        return isPickedUp && (
            !Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit dropHit, player.pickUpDistance)
            || (dropHit.collider != null && !dropHit.collider.CompareTag("Bullets"))
        );
    }


    /// <summary>
    /// Attempts to drop the currently held item (handles both online and offline modes).
    /// </summary>
    private void TryDropItem()
    {
        if (IsOnlineMode)
        {
            if (!IsOwner || heldItem == null) return;

            Item itemScript = heldItem.GetComponent<Item>();
            GameObject worldObject = itemScript?.afterSpawnPrefab;

            if (worldObject != null && player.holdPosition != null)
            {
                DropItemServerRpc(worldObject.name, player.holdPosition.position, player.holdPosition.rotation);
            }
        }
        else
        {
            DropItem();
        }
    }

    /// <summary>
    /// Drops the currently held item.
    /// </summary>
    public void DropItem()
    {
        if (heldItem == null) return;

        Item itemComponent = heldItem.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;
        Instantiate(itemComponent.afterSpawnPrefab, player.holdPosition.position, player.holdPosition.rotation); // Spawns the object back to world

        DestroyItem();  // Destroys the held Item
        ChangePickUpStatus(false);
    }

    [ServerRpc]
    public void DropItemServerRpc(string worldItemName, Vector3 droppingPosition, Quaternion droppingRotation, ServerRpcParams serverRpcParams = default)
    {
        if (networkPrefabsList == null || networkPrefabsList.networkItemsPrefabs.Count <= 0) return;
        foreach (var ntwObject in networkPrefabsList.networkItemsPrefabs)
        {
            if (ntwObject.name == worldItemName)
            {
                GameObject worldObject = Instantiate(ntwObject, droppingPosition, droppingRotation);
                worldObject.GetComponent<NetworkObject>()?.Spawn(); // Spawns the object back to world

                DestroyItemClientRpc(); // Destroys the held Item
                break;
            }
        }

    }

    #endregion

    #region Helper Functions

    /// <summary>
    /// Spawn item locally. (Offline mode)
    /// </summary>
    private void SpawnItem(GameObject itemObject)
    {
        if (itemObject == null) return;

        heldItem = Instantiate(itemObject, player.holdPosition);
        SetupItemTransform(heldItem, heldItem.GetComponent<Item>());
    }

    /// <summary>
    /// Spawn item in player hands on network across all clients. (Online mode)
    /// </summary>
    [ClientRpc]
    private void SpawnItemClientRpc(ulong objectId, ClientRpcParams clientRpcParams)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject itemNtwObject)) return;
        GameObject itemPrefab = itemNtwObject.GetComponent<Item>()?.afterSpawnPrefab;

        if (itemPrefab != null)
        {
            GameObject itemObject = Instantiate(itemPrefab, player.holdPosition);

            if (itemObject != null)
            {
                heldItem = itemObject;
                Item itemScript = heldItem.GetComponent<Item>();

                if (itemScript != null)
                {
                    itemScript.isPickedUp = true;
                    SetupItemTransform(itemObject, itemScript);
                }

                isPickedUp = true; // Set pick up status to true
            }
        }
    }

    [ClientRpc]
    private void SpawnItemClientRpc(ulong objectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject itemNtwObject)) return;
        GameObject itemPrefab = itemNtwObject.GetComponent<Item>()?.afterSpawnPrefab;

        if (itemPrefab != null)
        {
            GameObject itemObject = Instantiate(itemPrefab, player.holdPosition);

            if (itemObject != null)
            {
                heldItem = itemObject;
                Item itemScript = heldItem.GetComponent<Item>();

                if (itemScript != null)
                {
                    itemScript.isPickedUp = true;
                    SetupItemTransform(itemObject, itemScript);
                }


                isPickedUp = true;
            }
        }
    }

    /// <summary>
    /// Change pickup status locally (Offline mode)
    /// </summary>
    private void ChangePickUpStatus(bool status)
    {
        isPickedUp = status;
    }

    /// <summary>
    /// Destroy item locally by despawning it (Offline mode)
    /// </summary>
    private void DestroyItem()
    {
        if (heldItem != null)
        {
            Destroy(heldItem);
        }
    }

    /// <summary>
    /// Destroy item across all clients (Online mode)
    /// </summary>
    [ClientRpc]
    private void DestroyItemClientRpc()
    {
        if (heldItem != null)
        {
            Destroy(heldItem);
            isPickedUp = false;
        }
    }

    /// <summary>
    /// Creates ClientRpcParams for to change states only on a specific client (Online mode)
    /// </summary>
    private ClientRpcParams CreateClientRpcParams(ulong clientId)
    {
        return new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
    }

    /// <summary>
    /// Sets the transform of the held item
    /// </summary>
    private void SetupItemTransform(GameObject instance, Item itemComponent)
    {
        if (instance == null || itemComponent == null) return;

        instance.transform.localPosition = itemComponent.positionOffset;
        instance.transform.localRotation = Quaternion.Euler(itemComponent.rotationOffset);
        instance.transform.localScale = itemComponent.scaleOffset;
    }
    #endregion
}
