using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Reload : NetworkBehaviour
{
    [Header("Reload Settings")]
    [SerializeField] private int maxBullets = 2;
    [SerializeField] private float pickUpDistance = 8f;

    [Header("References")]
    [SerializeField] private Shoot shoot;
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;

    private RaycastHit hitInfo;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Update()
    {
        if (IsValidPlayer() && CanReload() && IsBullet())
            ReloadGun();
    }

    // <summary>
    // Validate player based on online or offline mode
    // </summary>
    private bool IsValidPlayer()
    {
        if (!IsOnlineMode) return true;
        return IsOwner;
    }

    // <summary>
    // Check if player can reload or not
    // </summary>
    private bool CanReload()
    {
        if (shoot != null && item != null && input != null)
        {
            if (item.isPickedUp && input.CheckPickUpPressed())
                if (shoot.currentBullets.Value < maxBullets)
                {
                    return true;
                }
                else
                {
                    Debug.Log("Gun is already full");
                    return false;
                }
        }
        return false;
    }

    // <summary>
    // Check if the item is bullet or not
    // </summary>
    private bool IsBullet()
    {
        if (item?.owner?.camera == null) return false; // Ensure the camera is not null
        Camera camera = item.owner.camera;

        // Perform a raycast in the direction camera is facing and checks if it hits an bulet or not
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo, pickUpDistance) &&
        hitInfo.collider.CompareTag("Bullets");
    }

    private void ReloadGun()
    {
        if (hitInfo.collider == null || shoot == null) return;

        if (IsOnlineMode)
            RemoveBulletFromWorldServerRpc(hitInfo.collider.gameObject.GetComponent<NetworkObject>());
        else
            Destroy(hitInfo.collider.gameObject);

        shoot.IncrementBullets(1);
    }

    // <summary>
    // Despawn and destroys the bullet on the server and the change reflects to all clients
    // </summary>
    [ServerRpc]
    public void RemoveBulletFromWorldServerRpc(NetworkObjectReference bulletRef)
    {
        if (!bulletRef.TryGet(out NetworkObject bulletObject)) return;
        bulletObject.Despawn(true);
    }
}