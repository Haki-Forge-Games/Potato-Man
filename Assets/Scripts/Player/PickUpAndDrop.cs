using UnityEngine;
using Unity.Netcode;

public class PickUpAndDrop : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float ItemTransFormSpeed = 20f;
    [SerializeField] private float pickupDistance = 50f;

    [Header("References")]
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Camera camera;
    [SerializeField] private ItemDatabase networkPrefabsList;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    private RaycastHit hit;
    private GameObject heldItem;
    private Rigidbody heldItemRigidbody;
    private Item heldItemScript;
    private bool isPickedUp = false;

    private void Update()
    {
        if (inputs?.CheckPickUpPressed() ?? false)
        {
            if (CanPickUpItem())
            {
                heldItem = hit.collider.gameObject;
                heldItemScript = heldItem?.GetComponent<Item>();
                heldItemRigidbody = heldItem?.GetComponent<Rigidbody>();

                if (heldItemRigidbody != null)
                {
                    heldItemRigidbody.isKinematic = true;
                    heldItemRigidbody.detectCollisions = false;
                }

                player.holdPosition.localPosition = heldItemScript.positionOffset;
                player.holdPosition.localRotation = Quaternion.Euler(heldItemScript.rotationOffset);

                heldItemScript.isPickedUp = true;
                isPickedUp = true;
            }
            else if (isPickedUp)
            {
                heldItemScript.isPickedUp = false;

                if (heldItemRigidbody != null)
                {
                    heldItemRigidbody.isKinematic = false;
                    heldItemRigidbody.detectCollisions = true;
                }

                heldItem = null;
                heldItemRigidbody = null;
                heldItemScript = null;

                isPickedUp = false;
            }
        }

        SoftParentToPlayer();
    }

    private void SoftParentToPlayer()
    {
        if (!isPickedUp) return;
        // Smooth position
        heldItem.transform.position = Vector3.Lerp(
            heldItem.transform.position,
            player.holdPosition.position,
            Time.deltaTime * ItemTransFormSpeed
        );

        // Smooth rotation
        heldItem.transform.rotation = Quaternion.Slerp(
            heldItem.transform.rotation,
            player.holdPosition.rotation,
            Time.deltaTime * ItemTransFormSpeed
        );

        // Smooth scale
        heldItem.transform.localScale = Vector3.Lerp(
          heldItem.transform.localScale,
          heldItemScript.scaleOffset,
          Time.deltaTime * ItemTransFormSpeed
      );

    }


    private bool CanPickUpItem()
    {
        if (camera == null) return false;
        if (isPickedUp) return false;
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, pickupDistance) &&
          hit.collider.CompareTag("PickAbles");
    }
}