using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAndDrop : MonoBehaviour
{
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;

    private RaycastHit hit;
    private GameObject heldItem;
    private Rigidbody heldItemRigidbody;
    private bool isPickedUp = false;

    private void Update()
    {

        if (inputs.CheckPickUpPressed())
        {
            if (RaycastCheck() && !isPickedUp)
            {
                PickUpItem();
            }
            else
            {
                if (!isPickedUp) return;
                DropItem();
            }


        }
    }

    private bool RaycastCheck()
    {
        if (playerCamera == null) return false;
        return Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, player.pickUpDistance) && hit.collider.gameObject.tag == "pickAbles";
    }

    private void PickUpItem()
    {
        if (hit.collider == null) return;

        GameObject item = hit.collider.gameObject;
        Rigidbody itemRigidbody = item.GetComponent<Rigidbody>();

        if (itemRigidbody == null || player.holdPosition == null) return;

        // Store the item and its Rigidbody
        heldItem = item;
        heldItemRigidbody = itemRigidbody;

        // Disable physics so it doesn't fall
        heldItemRigidbody.useGravity = false;
        heldItemRigidbody.isKinematic = true;

        // Attach the item to hold position
        heldItem.transform.SetParent(player.holdPosition);

        // sets holding offsets 
        heldItem.transform.localPosition = heldItem.GetComponent<Item>().positionOffset;
        heldItem.transform.localRotation = Quaternion.Euler(heldItem.GetComponent<Item>().rotationOffset);
        heldItem.transform.localScale = heldItem.GetComponent<Item>().scaleOffset;


        // heldItem.transform.Rotate(heldItem.GetComponent<Item>().rotationOffset);

        isPickedUp = true;
    }

    private void DropItem()
    {
        if (heldItem == null) return;

        // Re-enable physics so the item falls
        heldItemRigidbody.useGravity = true;
        heldItemRigidbody.isKinematic = false;

        // Detach the item from the player
        heldItem.transform.SetParent(null);

        // Apply a little force to make it look natural
        heldItemRigidbody.AddForce(transform.forward * 2f, ForceMode.Impulse);

        // Clear references
        heldItem = null;
        heldItemRigidbody = null;
        isPickedUp = false;
    }

}
