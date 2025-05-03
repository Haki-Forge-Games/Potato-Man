using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

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
        if (!IsValidPlayer()) return;
        if (inputs?.CheckPickUpPressed() ?? false)
        {
            if (!isPickedUp)
                PickUpItem();
            else
                DropItem();
        }

        UpdateHeldItemTransform();
    }

    private void PickUpItem()
    {
        if (!CanPickUpItem()) return;

        heldItem = hit.collider.gameObject;
        heldItemScript = heldItem.GetComponent<Item>();
        heldItemRigidbody = heldItem.GetComponent<Rigidbody>();

        // sync changes on server if in online mode 
        if (IsOnlineMode)
        {
            RequestOwnershipServerRpc(heldItem.GetComponent<NetworkObject>());
            HideHeldItemServerRpc(heldItem.GetComponent<NetworkObject>());
            SetItemPhysicsServerRpc(heldItem.GetComponent<NetworkObject>(), true);
        }
        else
        {
            // change locally if in offline mode 
            if (heldItemRigidbody != null)
            {
                heldItemRigidbody.isKinematic = true;
                heldItemRigidbody.detectCollisions = false;

            }
        }

        if (heldItemScript != null)
        {
            heldItemScript.SetOwner(player);
            heldItemScript.SetPickedUpState(true);
        }

        if (holdPosition != null)
        {
            holdPosition.localPosition = heldItemScript.positionOffset;
            holdPosition.localRotation = Quaternion.Euler(heldItemScript.rotationOffset);
        }

        itemOriginalScale = heldItem.transform.localScale;

        isPickedUp = true;
    }

    private void DropItem()
    {
        if (heldItem == null) return;

        if (heldItemScript != null)
        {
            heldItemScript.SetPickedUpState();
            heldItemScript.SetOwner();
        }

        // sync changes on server if in online mode 
        if (IsOnlineMode)
        {
            SetItemPhysicsServerRpc(heldItem.GetComponent<NetworkObject>(), false);
            RemoveOwnershipServerRpc(heldItem.GetComponent<NetworkObject>());
            ShowHideHeldItemServerRpc(heldItem.GetComponent<NetworkObject>());
        }
        else
        {
            // change locally if in offline mode 
            if (heldItemRigidbody != null)
            {
                heldItemRigidbody.isKinematic = false;
                heldItemRigidbody.detectCollisions = true;
            }
        }

        heldItem.transform.localScale = itemOriginalScale; // reseting the scale

        // Clear references
        heldItem = null;
        heldItemRigidbody = null;
        heldItemScript = null;

        isPickedUp = false;

        // Reset holdPosition back to original
        if (holdPosition != null)
        {
            holdPosition.localPosition = holdPositionOriginalLocalPos;
            holdPosition.localRotation = holdPositionOriginalLocalRot;
        }
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

    private bool IsValidPlayer()
    {
        if (!IsOnlineMode) return true;
        return IsOwner;
    }


    private bool CanPickUpItem()
    {
        if (camera == null) return false;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        if (!Physics.Raycast(ray, out hit, pickupDistance)) return false;

        Collider hitCollider = hit.collider;
        if (hitCollider == null || !hitCollider.CompareTag("PickAbles")) return false;

        GameObject hitObject = hitCollider.gameObject;
        if (hitObject == null) return false;

        Item itemScript = hitObject.GetComponent<Item>();
        return itemScript != null && !isPickedUp && !itemScript.isPickedUp;
    }

    [ServerRpc]
    private void RequestOwnershipServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetworkObject)) return;
        itemNetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

    }

    [ServerRpc]
    private void RemoveOwnershipServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetworkObject)) return;
        itemNetworkObject.RemoveOwnership();
    }

    [ServerRpc]
    private void SetItemPhysicsServerRpc(NetworkObjectReference itemRef, bool isHeld)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        Rigidbody rb = itemNetObj.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = isHeld;
        rb.detectCollisions = !isHeld;

        // perform changes on all clients 
        SetItemPhysicsClientRpc(itemRef, isHeld);
    }

    [ClientRpc]
    private void SetItemPhysicsClientRpc(NetworkObjectReference itemRef, bool isHeld)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        Rigidbody rb = itemNetObj.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = isHeld;
        rb.detectCollisions = !isHeld;
    }

    [ServerRpc]
    private void HideHeldItemServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        var allClientIds = NetworkManager.Singleton.ConnectedClientsIds;
        var targetClients = new List<ulong>();

        foreach (var clientId in allClientIds)
        {
            if (clientId != serverRpcParams.Receive.SenderClientId)
            {
                targetClients.Add(clientId);
            }
        }

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        };

        HideHeldItemClientRpc(itemRef, clientRpcParams);
    }

    [ClientRpc]
    private void HideHeldItemClientRpc(NetworkObjectReference itemRef, ClientRpcParams clientRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;
        itemNetObj.gameObject.SetActive(false);
    }


    [ServerRpc]
    private void ShowHideHeldItemServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        var allClientIds = NetworkManager.Singleton.ConnectedClientsIds;
        var targetClients = new List<ulong>();

        foreach (var clientId in allClientIds)
        {
            if (clientId != serverRpcParams.Receive.SenderClientId)
            {
                targetClients.Add(clientId);
            }
        }

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        };

        ShowHideHeldItemClientRpc(itemRef, clientRpcParams);
    }

    [ClientRpc]
    private void ShowHideHeldItemClientRpc(NetworkObjectReference itemRef, ClientRpcParams clientRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;
        itemNetObj.gameObject.SetActive(true);
    }

}
