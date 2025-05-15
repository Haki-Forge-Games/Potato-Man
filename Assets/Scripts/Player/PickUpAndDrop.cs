using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

public class PickUpAndDrop : NetworkBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float itemTransformSpeed = 20f;
    [SerializeField] private float pickupDistance = 8f;

    [Header("References")]
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera camera;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform teamViewItemHolder;
    [SerializeField] private TwoBoneIKConstraint ikConstraintLeftHand;
    [SerializeField] private TwoBoneIKConstraint ikConstraintRightHand;

    [Header("Left Hand")]
    public Transform leftTarget;
    public Transform leftHint;

    [Header("Right Hand")]
    public Transform rightTarget;
    public Transform rightHint;

    private RaycastHit hit;
    private GameObject heldItem;
    private Item heldItemScript;

    private Vector3 holdPositionOriginalLocalPos;
    private Quaternion holdPositionOriginalLocalRot;
    private Vector3 itemOriginalScale;

    private GameObject teammateViewObject;

    private bool isPickedUp = false;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        // Save original holdPosition transform values
        holdPositionOriginalLocalPos = holdPosition.localPosition;
        holdPositionOriginalLocalRot = holdPosition.localRotation;
    }

    private void Update()
    {
        if (!IsValidPlayer()) return; // Check if the player is valid to pick up items
        if (inputs?.CheckPickUpPressed() ?? false)
        {
            if (!isPickedUp)
                PickUpItem();
            else
                DropItem();
        }

        UpdateHeldItemTransform();
    }


    // <Summary>
    // Updates the transform of the held item
    // </Summary>
    private void UpdateHeldItemTransform()
    {
        if (!isPickedUp || heldItem == null || heldItemScript == null) return;

        // // Smooth position
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


    // <Summary>
    // Pick up item if the player is valid and the item is not already picked up
    // </Summary>
    private void PickUpItem()
    {
        if (!CanPickUpItem()) return;

        heldItem = hit.collider.gameObject;
        heldItemScript = heldItem.GetComponent<Item>();

        // sync changes on server if in online mode 
        if (IsOnlineMode)
            PickUpServerRpc(heldItem.GetComponent<NetworkObject>());
        else
            SetUpPhysics(heldItem, true); // change locally if in offline mode


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


    // <Summary>
    // Drop the item if it is held
    // </Summary>
    private void DropItem()
    {
        if (heldItem == null) return;

        isPickedUp = false;

        if (heldItemScript != null)
        {
            heldItemScript.SetPickedUpState();
            heldItemScript.SetOwner();
        }

        // sync changes on server if in online mode 
        if (IsOnlineMode)
            DropItemServerRpc(heldItem.GetComponent<NetworkObject>(), itemOriginalScale);
        else
            SetUpPhysics(heldItem, false); // change locally if in offline mode

        // Clear references
        heldItem = null;
        heldItemScript = null;


        // Reset holdPosition back to original
        if (holdPosition != null)
        {
            holdPosition.localPosition = holdPositionOriginalLocalPos;
            holdPosition.localRotation = holdPositionOriginalLocalRot;
        }
    }

    // <Summary>
    // Pick up item on server and sync changes to all clients
    // </Summary>
    // <param name="itemRef">The item reference</param>

    [ServerRpc]
    private void PickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        // Getting all clients except the one who picked up the item
        var allClientIds = GetAllClientIdsExcept(serverRpcParams.Receive.SenderClientId);
        var clientRpcParams = new ClientRpcParams  // Clinet Rpc params for sending to all clients except the owner
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds
            }
        };

        // Setting up Ownerships
        itemNetObj.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

        // Setting up Physics
        SetUpPhysics(itemNetObj.gameObject, true);
        SetItemPhysicsClientRpc(itemRef, true);

        HandleHeldItemVisibilityClientRpc(itemRef, false, clientRpcParams);  // Setting up Visibility
        SpawnTeammateViewItemClientRpc(itemRef, clientRpcParams);  // Spawning a non network obj for all clients except the owner one 

    }

    // <Summary>
    // Drop item on server and sync changes to all clients
    // </Summary>
    [ServerRpc]
    private void DropItemServerRpc(NetworkObjectReference itemRef, Vector3 itemOriginalScale, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;

        // Getting all clients except the one who picked up the item
        var allClientIds = GetAllClientIdsExcept(serverRpcParams.Receive.SenderClientId);
        var clientRpcParams = new ClientRpcParams  // Clinet Rpc params for sending to all clients except the owner
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds
            }
        };


        DestroyTeammateViewItemClientRpc(clientRpcParams);  // Destroying the non network obj for all clients except the owner one

        // Setting up Physics                                
        SetUpPhysics(itemNetObj.gameObject, false);
        SetItemPhysicsClientRpc(itemRef, false);

        itemNetObj.transform.localScale = itemOriginalScale; // Resetting the scale to original
        HandleHeldItemVisibilityClientRpc(itemRef, true, clientRpcParams);  // Setting up Visibility
        itemNetObj.RemoveOwnership(); // Setting up Ownerships
    }


    // <Summary>
    // Checks if the player can pick up the item
    // </Summary>
    // <returns>True if the player can pick up the item, false otherwise</returns>
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

    // <Summary>
    // Sets up the physics for the item specifies for all clients
    // </Summary>
    [ClientRpc]
    private void SetItemPhysicsClientRpc(NetworkObjectReference itemRef, bool state)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;
        SetUpPhysics(itemNetObj.gameObject, state);
    }

    // <Summary>
    // Sets up the visibility for the item specifies for specific clients
    // </Summary>
    // <param name="itemRef">The item reference</param>
    // <param name="state">The state in which the item visibility needs to be set up</param>
    // <param name="clientRpcParams">The client rpc params to perform rpc on specific clients</param>
    [ClientRpc]
    private void HandleHeldItemVisibilityClientRpc(NetworkObjectReference itemRef, bool state, ClientRpcParams clientRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;
        itemNetObj.gameObject.GetComponent<MeshRenderer>().enabled = state;
    }


    // <Summary>
    // Spawns the teammate view item for all clients except the owner
    // </Summary>
    // <param name="itemRef">The item reference</param>
    // <param name="clientRpcParams">The client rpc params to perform rpc on specific clients</param>
    [ClientRpc]
    private void SpawnTeammateViewItemClientRpc(NetworkObjectReference itemRef, ClientRpcParams clientRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNetObj)) return;
        var teammateViewItemPrefab = itemNetObj.GetComponent<Item>()?.teammateViewItemPrefab;

        if (teammateViewItemPrefab != null)
        {
            GameObject teammateViewItem = Instantiate(teammateViewItemPrefab, teamViewItemHolder.position, teamViewItemHolder.rotation);

            if (teammateViewItem == null || teamViewItemHolder == null) return;

            teammateViewItem.transform.SetParent(teamViewItemHolder); // Parenting the item to the holder
            var teammateViewItemScript = teammateViewItem.GetComponent<TeamViewItem>();

            if (teammateViewItemScript != null)
            {
                // Setting up the item transform
                teammateViewItem.transform.localPosition = teammateViewItemScript.positionOffset;
                teammateViewItem.transform.localRotation = Quaternion.Euler(teammateViewItemScript.rotationOffset);
                teammateViewItem.transform.localScale = teammateViewItemScript.scaleOffset;

                // Setting up the target and hint transforms
                SetTargetAndHintTransform(leftTarget, leftHint, teammateViewItemScript.leftTarget, teammateViewItemScript.leftHint); // left hand
                SetTargetAndHintTransform(rightTarget, rightHint, teammateViewItemScript.rightTarget, teammateViewItemScript.rightHint); // right hand

                // setting weights 
                HandleHandWeights(ikConstraintLeftHand, 1f); // left hand 
                HandleHandWeights(ikConstraintRightHand, 1f); // right hand 
            }

            teammateViewObject = teammateViewItem;
        }

    }

    // <Summary>
    // Destroys the teammate view item for all clients except the owner
    // </Summary>
    // <param name="clientRpcParams">The client rpc params to perform rpc on specific clients</param>
    [ClientRpc]
    private void DestroyTeammateViewItemClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (teammateViewObject != null)
        {
            // Reset weights
            HandleHandWeights(ikConstraintLeftHand, 0f); // left hand 
            HandleHandWeights(ikConstraintRightHand, 0f); // right hand 

            Destroy(teammateViewObject);
            teammateViewObject = null;
        }

    }

    // <Summary>
    // Checks if the player is valid to pick up items for online and offline modes
    // </Summary>
    private bool IsValidPlayer()
    {
        if (!IsOnlineMode) return true;
        return IsOwner;
    }

    // <Summary>
    // Sets up the physics for the item specifies
    // </Summary>
    // <param name="item">The Item which physics needs to be set up</param>
    // <param name="state">The state in which the item physics needs to be set up</param>
    private void SetUpPhysics(GameObject item, bool state = false)
    {
        if (item == null) return;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = state;
        rb.detectCollisions = !state;
    }

    // <Summary>
    // Gets all client IDs except the one specified
    // </Summary>
    // <param name="excludedClientId">The client ID to exclude</param>
    private List<ulong> GetAllClientIdsExcept(ulong excludedClientId)
    {
        var allClientIds = NetworkManager.Singleton.ConnectedClientsIds;
        var targetClients = new List<ulong>();

        foreach (var clientId in allClientIds)
        {
            if (clientId != excludedClientId)
            {
                targetClients.Add(clientId);
            }
        }

        return targetClients;
    }

    // <Summary>
    // Gets all client IDs except the one specified
    // </Summary>
    // <param name="ikConstraint">The constrain component</param>
    // <param name="weight">The value of the weight btw 0 and 1</param>
    private void HandleHandWeights(TwoBoneIKConstraint ikConstraint, float weight)
    {
        if (ikConstraint == null) return;

        ikConstraint.data.targetPositionWeight = weight;
        ikConstraint.data.targetRotationWeight = weight;
        ikConstraint.data.hintWeight = weight;
    }


    // <Summary>
    // Sets the target and hint transforms for the item
    // </Summary>
    // <param name="target">The target transform</param>
    // <param name="hint">The hint transform</param>
    // <param name="newTarget">The new target transform</param>
    // <param name="newHint">The new hint transform</param>
    private void SetTargetAndHintTransform(Transform target, Transform hint, Transform newTarget, Transform newHint)
    {
        if (target == null || hint == null || newTarget == null || newHint == null) return;

        target.position = newTarget.position;
        target.rotation = newTarget.rotation;

        hint.position = newHint.position;
        hint.rotation = newHint.rotation;
    }

}
