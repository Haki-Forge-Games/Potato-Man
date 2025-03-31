using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Reload : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Shoot shoot;
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
        if (player == null) return false; // check if player is not null 

        if (!IsOnlineMode) return true; // Check is online or offline 
        return player.IsOwner; // Check if the player is owner if in online mode
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

        shoot.currentBullets += 1;
        Debug.Log("Reload Complete");
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