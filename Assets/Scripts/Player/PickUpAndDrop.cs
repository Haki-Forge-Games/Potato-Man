using UnityEngine;
using Unity.Netcode;

public class PickUpAndDrop : NetworkBehaviour
{
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private NetworkPickAnsDrop networkPickAnsDrop;

    private RaycastHit hit;
    public bool isPickedUp { get; set; } = false;

    private void Update()
    {
        if (!IsOwner) return;

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

    private bool CanPickUpItem()
    {
        return playerCamera != null &&
               Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, player.pickUpDistance) &&
               hit.collider.CompareTag("pickAbles");
    }

    private void TryPickUpItem()
    {
        NetworkObject networkObject = hit.collider.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkPickAnsDrop.PickUpServerRpc(networkObject);
        }
    }

    private void TryDropItem()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkPickAnsDrop.DropItemServerRpc(networkObject);
        }
    }

}
