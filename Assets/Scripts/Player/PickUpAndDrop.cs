using UnityEngine;
using Unity.Netcode;

public class PickUpAndDrop : NetworkBehaviour
{
    #region Variables
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private NetworkPickAnsDrop networkPickAnsDrop;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    private RaycastHit hit;
    private GameObject heldItem;
    public bool isPickedUp { get; set; } = false;
    #endregion

    #region Update Loop
    // Checks for input and handles pick up or drop actions
    private void Update()
    {
        if (inputs.CheckPickUpPressed())
        {
            if (CanPickUpItem())
            {
                TryPickUpItem();
            }
            else if (isPickedUp)
            {
                TryDropItem();
            }
        }
    }
    #endregion

    #region Pickup Logic
    // Checks if the player can pick up an item
    private bool CanPickUpItem()
    {
        return playerCamera != null &&
               Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, player.pickUpDistance) &&
               hit.collider.CompareTag("pickAbles") && !isPickedUp;
    }

    // Attempts to pick up an item
    private void TryPickUpItem()
    {
        if (IsOnlineMode)
        {
            if (!IsOwner) return;

            NetworkObject networkObject = hit.collider.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkPickAnsDrop.PickUpServerRpc(networkObject);
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

    // Picks up an item and attaches it to the player
    public void PickUpItem(GameObject itemObject)
    {
        if (itemObject == null) return;

        Item itemComponent = itemObject.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        SpawnItem(itemComponent.afterSpawnPrefab);
        Destroy(itemObject);
        ChangePickUpStatus(true);
    }
    #endregion

    #region Drop Logic
    // Attempts to drop the currently held item
    private void TryDropItem()
    {
        if (IsOnlineMode)
        {
            if (!IsOwner) return;

            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkPickAnsDrop.DropItemServerRpc(networkObject);
            }
        }
        else
        {
            DropItem();
        }
    }

    // Drops the currently held item
    public void DropItem()
    {
        if (heldItem == null) return;

        Item itemComponent = heldItem.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        Instantiate(itemComponent.afterSpawnPrefab, player.holdPosition.position, player.holdPosition.rotation);
        DestroyItem();
        ChangePickUpStatus(false);
    }
    #endregion

    #region Helper Functions
    // Spawns an item in the player's hand
    private void SpawnItem(GameObject itemObject)
    {
        if (itemObject == null || player == null) return;

        heldItem = Instantiate(itemObject);
        Item itemComponent = heldItem.GetComponent<Item>();

        if (heldItem == null || itemComponent == null) return;

        heldItem.transform.SetParent(player.holdPosition);
        SetupItemTransform(heldItem, itemComponent);
        itemComponent.isPickedUp = true;
    }

    // Updates the pickup status
    private void ChangePickUpStatus(bool status)
    {
        isPickedUp = status;
    }

    // Destroys the held item
    private void DestroyItem()
    {
        if (heldItem != null)
        {
            Destroy(heldItem);
        }
    }

    // Adjusts the item's transform settings when picked up
    private void SetupItemTransform(GameObject instance, Item itemComponent)
    {
        instance.transform.localPosition = itemComponent.positionOffset;
        instance.transform.localRotation = Quaternion.Euler(itemComponent.rotationOffset);
        instance.transform.localScale = itemComponent.scaleOffset;
    }
    #endregion
}