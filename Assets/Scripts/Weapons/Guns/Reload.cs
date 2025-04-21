using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Reload : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Inputs inputs;

    private float pickUpDistance;
    private Camera camera;
    private RaycastHit hit;
    private Player player;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        GameObject grandGrandparentObj = GetGrandGrandparentObject();
        if (grandGrandparentObj == null) return;

        player = grandGrandparentObj.GetComponent<Player>();
        pickUpDistance = player?.pickUpDistance ?? 10f;

        GameObject grandparentObj = GetGrandparentObject();
        if (grandparentObj == null) return;

        camera = grandparentObj.GetComponent<Camera>();
    }

    private void Update()
    {
        if (inputs.CheckPickUpPressed() && IsValidPlayer() && CheckIsBullet())
        {
            if (gun == null) return;
            if (gun.currentBullets < gun.maxBullets)
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
        if (player == null) return false;

        if (!IsOnlineMode) return true;
        return player.IsOwner;
    }

    private bool CheckIsBullet()
    {
        if (camera == null) return false;
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, pickUpDistance) && hit.collider.CompareTag("Bullets");
    }

    private void ReloadGun()
    {
        if (IsOnlineMode)
        {
            NetworkObject bulletNtwObject = hit.collider.gameObject.GetComponent<NetworkObject>();
            if (bulletNtwObject == null) return;

            // remove bullet from world form all clients 
            player?.RemoveBulletFromWorldServerRpc(bulletNtwObject);
        }
        else
        {
            Destroy(hit.collider.gameObject);
        }

        if (gun == null) return;
        gun.currentBullets += 1;
    }

    private GameObject GetGrandparentObject()
    {
        return transform.parent?.parent?.gameObject;
    }

    private GameObject GetGrandGrandparentObject()
    {
        return transform.parent?.parent?.parent?.gameObject;
    }
}