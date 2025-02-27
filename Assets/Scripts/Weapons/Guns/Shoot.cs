using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Inputs input;
    private RaycastHit hit;

    public Camera camera { get; set; }


    private void Update()
    {
        if (input != null && input.CheckShootPressed())
        {
            SpreadPellets();
        }
    }

    private void HandleShoot()
    {

    }

    private bool CheckIsEnemy()
    {
        if (camera == null) return false;
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, gun.shootRange) && hit.collider.gameObject.tag == "enemy";
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
}
