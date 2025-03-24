using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Shoot : MonoBehaviour
{
    public int currentBullets { get; set; } = 2;

    [SerializeField] private Gun gun;
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;

    private Player player;
    private Camera camera;
    private RaycastHit hit;
    private float lastShotTime = 0.0f;
    private const string SHOOT_ANIMATION = "Shoot";
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        GameObject grandGrandparentObj = GetGrandGrandparentObject();
        if (grandGrandparentObj == null) return;

        player = grandGrandparentObj.GetComponent<Player>();

        GameObject grandparentObj = GetGrandparentObject();
        if (grandparentObj == null) return;

        camera = grandparentObj.GetComponent<Camera>();
    }

    private void Update()
    {
         Debug.Log("Shoot script exists on client: " + NetworkManager.Singleton.LocalClientId);

        if (!IsValidPlayer()) return;
        if (CanShoot())
        {
            HandleShoot();
            lastShotTime = Time.time;
        }
    }

    private bool IsValidPlayer()
    {
        if (player == null) return false; // check if player is not null 

        if (!IsOnlineMode) return true; // Check is online or offline 
        return player.IsOwner; // Check if the player is owner if in online mode
    }

    private bool CanShoot()
    {
        return input != null && item != null && gun != null && item.isPickedUp && gun.fireRate > 0 && input.CheckShootPressed() && Time.time >= lastShotTime + gun.fireRate;
    }

    private void HandleShoot()
    {
        if (currentBullets <= 0)
        {
            Debug.Log("Gun is empty");
            return;
        }

        currentBullets--; // reducnng bullets with per shot 

        PlayMuzzleFlash();
        ShakeScreen();
        HandleAnimator();

        if (IsEnemy() && hit.collider != null)
        {
            StateManager controller = hit.collider.gameObject.GetComponent<StateManager>();
            if (controller == null) return;

            if (IsOnlineMode)
            {
                NetworkObject ntwObject = hit.collider.gameObject.GetComponent<NetworkObject>();

                if (ntwObject == null) return;
                player.ChangeStateServerRpc(ntwObject);
            }
            else
            {
                // set enemy to death state 
                controller.ChangeStateToDeath();
            }

        }
    }

    private bool IsEnemy()
    {
        if (gun == null || camera == null) return false;
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, gun.shootRange) && hit.collider.CompareTag("Enemy");
    }

    private void PlayMuzzleFlash()
    {
        if (gun.muzzleFlash != null)
        {
            gun.muzzleFlash.Play();
        }
    }

    private void ShakeScreen()
    {
        GameObject grandparentObj = GetGrandparentObject();
        if (grandparentObj == null) return;

        ScreenShake screenShake = grandparentObj.GetComponent<ScreenShake>();
        if (screenShake != null)
        {
            StartCoroutine(screenShake.Shake(Mathf.Max(gun.impactMagnitude, 0f), Mathf.Max(gun.impactDuration, 0f)));
        }
    }

    private void HandleAnimator()
    {
        if (gun.animator != null)
        {
            gun.animator.Rebind();
            gun.animator.Play(SHOOT_ANIMATION);
        }
    }

    private GameObject GetGrandGrandparentObject()
    {
        return transform.parent?.parent?.parent?.gameObject;
    }

    private GameObject GetGrandparentObject()
    {
        return transform.parent?.parent?.gameObject;
    }

    // network 
}
