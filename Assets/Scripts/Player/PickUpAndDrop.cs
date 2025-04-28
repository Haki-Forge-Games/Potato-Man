using UnityEngine;
using Unity.Netcode;

public class PickUpAndDrop : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float itemTransformSpeed = 20f;
    [SerializeField] private float pickupDistance = 50f;

    [Header("References")]
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Camera camera;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private RaycastHit hit;
    private GameObject heldItem;
    private Rigidbody heldItemRigidbody;
    private Item heldItemScript;
    private bool isPickedUp = false;

    private Vector3 itemOriginalScale;
    private Vector3 holdPositionOriginalLocalPos;
    private Quaternion holdPositionOriginalLocalRot;

    private void Start()
    {
        // Save original holdPosition transform values
        holdPositionOriginalLocalPos = holdPosition.localPosition;
        holdPositionOriginalLocalRot = holdPosition.localRotation;
    }

    private void Update()
    {
        if (inputs?.CheckPickUpPressed() ?? false)
        {
            if (!isPickedUp)
                PickUpItem();
            else{}
                // DropItem();
        }

        UpdateHeldItemTransform();
    }

    private void PickUpItem()
    {
        if (!CanPickUpItem()) return;

        heldItem = hit.collider.gameObject;
        heldItemScript = heldItem.GetComponent<Item>();
        heldItemRigidbody = heldItem.GetComponent<Rigidbody>();

        if (IsOnlineMode)
        {
            NetworkObject itemNtwObject = heldItem.GetComponent<NetworkObject>();
            if (itemNtwObject == null) return;

            RequestOwnershipServerRpc(itemNtwObject);
        }

        if (heldItemRigidbody != null)
        {
            heldItemRigidbody.isKinematic = true;
            heldItemRigidbody.detectCollisions = false;
        }


        itemOriginalScale = heldItem.transform.localScale;

        holdPosition.localPosition = heldItemScript.positionOffset;
        holdPosition.localRotation = Quaternion.Euler(heldItemScript.rotationOffset);

        heldItemScript?.SetOwner(player);
        heldItemScript?.SetPickedUpState(true);

        isPickedUp = true;
    }

    private void DropItem()
    {
        heldItemScript?.SetPickedUpState();
        heldItemScript?.SetOwner();

        if (heldItemRigidbody != null)
        {
            heldItemRigidbody.isKinematic = false;
            heldItemRigidbody.detectCollisions = true;
        }


        if (heldItem != null)
            heldItem.transform.localScale = itemOriginalScale;


        if (IsOnlineMode)
        {
            NetworkObject itemNtwObject = heldItem.GetComponent<NetworkObject>();
            if (itemNtwObject == null) return;

            RemoveOwnershipServerRpc(itemNtwObject);
        }

        // Clear references
        heldItem = null;
        heldItemRigidbody = null;
        heldItemScript = null;
        isPickedUp = false;

        // Reset holdPosition back to original
        holdPosition.localPosition = holdPositionOriginalLocalPos;
        holdPosition.localRotation = holdPositionOriginalLocalRot;
    }

    private void UpdateHeldItemTransform()
    {
        if (!isPickedUp || heldItem == null || heldItemScript == null) return;

        // Smooth position
        heldItem.transform.position = Vector3.Lerp(
            heldItem.transform.position,
            holdPosition.position,
            Time.deltaTime * itemTransformSpeed
        );

        // Smooth rotation
        heldItem.transform.rotation = Quaternion.Slerp(
            heldItem.transform.rotation,
            holdPosition.rotation,
            Time.deltaTime * itemTransformSpeed
        );

        // Smooth scale
        heldItem.transform.localScale = Vector3.Lerp(
            heldItem.transform.localScale,
            heldItemScript.scaleOffset,
            Time.deltaTime * itemTransformSpeed
        );
    }

    private bool CanPickUpItem()
    {
        if (camera == null) return false;

        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, pickupDistance) &&
               hit.collider.CompareTag("PickAbles");
    }

    [ServerRpc]
    private void RequestOwnershipServerRpc(NetworkObjectReference itemReference)
    {
        if (itemReference.TryGet(out NetworkObject itemNetworkObject))
        {
            itemNetworkObject.ChangeOwnership(OwnerClientId);
        }
    }

    [ServerRpc]
    private void RemoveOwnershipServerRpc(NetworkObjectReference itemReference)
    {
        if (itemReference.TryGet(out NetworkObject itemNetworkObject))
        {
            itemNetworkObject.RemoveOwnership();
        }
    }
}
