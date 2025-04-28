using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Reload : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Gun gun;
    [SerializeField] private Shoot shoot;
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;

    private RaycastHit hitInfo;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Update()
    {
        if (!IsValidPlayer()) return;
        if (CanReload() && IsBullet())
        {
            if (shoot.currentBullets < gun.maxBullets)
            {
                ReloadGun();
            }
            else
            {
                Debug.Log("Gun is full");
            }

        }
    }

    private bool IsValidPlayer()
    {
        if (!IsOnlineMode) return true;
        return item?.owner?.IsOwner ?? false;
    }

    private bool CanReload()
    {
        if (gun == null || input == null || item == null || shoot == null) return false;
        if (!input.CheckPickUpPressed()) return false;
        return true;
    }

    private bool IsBullet()
    {
        if (item?.owner?.camera == null) return false;
        Transform cameraTransform = item.owner.camera.transform;
        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, item.owner.pickUpDistance) && (hitInfo.collider?.CompareTag("Bullets") ?? false);
    }

    private void ReloadGun()
    {
        if (hitInfo.collider?.gameObject == null) return;

        if (IsOnlineMode)
        {
            NetworkObject bulletNtwObject = hitInfo.collider.gameObject.GetComponent<NetworkObject>();

            if (bulletNtwObject == null) return;
            RemoveBulletFromWorldServerRpc(bulletNtwObject);
        }
        else
        {
            Destroy(hitInfo.collider.gameObject);
        }

        shoot.currentBullets += 1;
    }

    [ServerRpc]
    public void RemoveBulletFromWorldServerRpc(NetworkObjectReference bulletRef)
    {
        if (!bulletRef.TryGet(out NetworkObject bulletObject)) return;
        bulletObject.Despawn(true);
    }
}