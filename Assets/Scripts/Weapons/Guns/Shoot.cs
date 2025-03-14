using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;

    private RaycastHit hit;
    private float lastShotTime = 0.0f;  // Track last shot time
    private const string SHOOT_ANIMATION = "Shoot";

    private void Update()
    {
        GameObject grandparentObj = gameObject.transform.parent?.parent?.parent?.gameObject;
        
        if (grandparentObj == null) return;
        Player player = grandparentObj.GetComponent<Player>();
        if (player == null) return;
        if (!player.IsOwner) return;

        if (input != null && item != null && gun != null && item.isPickedUp && gun.fireRate != null && input.CheckShootPressed())
        {
            if (Time.time >= lastShotTime + gun.fireRate) // Check if enough time has passed
            {
                HandleShoot();
                lastShotTime = Time.time; // Update last shot time
            }
        }
    }

    private void HandleShoot()
    {
        PlayMuzzleFlash();
        ShakeScreen();
        HandleAnimator();
    }

    private bool CheckIsEnemy()
    {
        if (gun == null || gun.GetComponent<Camera>() == null) return false;
        return Physics.Raycast(gun.GetComponent<Camera>().transform.position, gun.GetComponent<Camera>().transform.forward, out hit, gun.shootRange) && hit.collider.gameObject.tag == "enemy";
    }

    private void PlayMuzzleFlash()
    {
        if (gun == null || gun.muzzleFlash != null)
        {
            gun.muzzleFlash.Play();
        }
    }

    private void ShakeScreen()
    {
        GameObject grandparentObj = gameObject.transform.parent?.parent?.gameObject;
        if (grandparentObj == null) return;
        ScreenShake screenShake = grandparentObj.GetComponent<ScreenShake>();
        if (screenShake == null) return;
        StartCoroutine(screenShake.Shake(gun.impactMagnitude, gun.impactDuration));
    }

    private void HandleAnimator()
    {
        if (gun == null || gun.animator == null) return;

        gun.animator.Rebind();
        gun.animator.Play(SHOOT_ANIMATION);
    }
}
