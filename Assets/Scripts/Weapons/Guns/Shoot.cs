using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Inputs input;
    private RaycastHit hit;


    private void Update()
    {
        if (input != null && input.CheckShootPressed())
        {
            HandleShoot();
        }
    }

    private void HandleShoot()
    {
        PlayMuzzleFlash();
        ShakeScreen();
        SpreadPellets();
    }

    private bool CheckIsEnemy()
    {
        if (gun == null || gun.camera == null) return false;
        return Physics.Raycast(gun.camera.transform.position, gun.camera.transform.forward, out hit, gun.shootRange) && hit.collider.gameObject.tag == "enemy";
    }

    private void SpreadPellets()
    {
        if (gun == null || gun.pelletPrefab == null || gun.spreadAngle == null || gun.firePoint == null || gun.pelletCount == null) return;

        for (int i = 0; i < gun.pelletCount; i++)
        {
            // Calculate spread (small offset in X, Y and z)
            Vector3 spread = new Vector3(
                Random.Range(-gun.spreadAngle, gun.spreadAngle),
                Random.Range(-gun.spreadAngle, gun.spreadAngle),
                Random.Range(-gun.spreadAngle, gun.spreadAngle)
            );

            // Instantiate pellet at firePoint
            GameObject pellet = Instantiate(gun.pelletPrefab, gun.firePoint);

            // Apply force in firePoint's forward direction + spread
            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 shootDirection = gun.firePoint.forward + spread; // Slightly modify direction
                rb.AddForce(shootDirection.normalized * 15f, ForceMode.Impulse);
            }
        }
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
        if (gun == null || gun.camera == null) return;

        ScreenShake screenShakeScript = gun.camera.GetComponent<ScreenShake>();

        if (screenShakeScript == null) return;

        StartCoroutine(screenShakeScript.Shake(gun.impactMagnitude, gun.impactDuration));
    }
}
