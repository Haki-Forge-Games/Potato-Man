using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Shoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Gun gun;
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;

    public int currentBullets { get; set; } = 2;
    private float lastShotTime = 0f;
    private RaycastHit hitInfo;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    private const string SHOOT_ANIMATION = "Shoot";

    private void Update()
    {
        if (!IsValidPlayer()) return;
        if (CanShoot())
        {
            ShootGun();
            lastShotTime = Time.time;
        }
    }

    private bool IsValidPlayer()
    {
        if (!IsOnlineMode) return true;
        return item?.owner?.IsOwner ?? false;
    }

    private bool CanShoot()
    {
        if (gun == null || input == null || item == null) return false;
        if (!input.CheckShootPressed()) return false;
        if (!item.isPickedUp) return false;
        if (Time.time <= lastShotTime + gun.fireRate) return false;
        return true;
    }

    private void ShootGun()
    {
        if (currentBullets <= 0)
        {
            Debug.Log("Gun is empty");
            return;
        }

        currentBullets--;

        PlayMuzzleFlash();
        ShakeCamera();

        if (IsEnemyHit())
        {
            if (hitInfo.collider?.gameObject == null) return;
            HandleEnemyHit(hitInfo.collider.gameObject);
        }
    }

    private bool IsEnemyHit()
    {
        if (gun == null || item?.owner?.camera == null) return false;

        var cameraTransform = item.owner.camera.transform;
        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, gun.shootRange) &&
               hitInfo.collider.CompareTag("Enemy");
    }

    private void HandleEnemyHit(GameObject enemy)
    {
        StateManager stateManager = enemy.GetComponent<StateManager>();
        if (stateManager == null) return;

        if (IsOnlineMode)
        {
            NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
            if (networkObject == null) return;

            item.owner.ChangeStateServerRpc(networkObject);
        }
        else
        {
            stateManager.ChangeStateToDeath();
        }
    }

    private void PlayMuzzleFlash()
    {
        if (gun?.muzzleFlash == null) return;
        gun.muzzleFlash.Play();
    }

    private void ShakeCamera()
    {
        if (item?.owner?.camera == null) return;

        ScreenShake screenShake = item.owner.camera.GetComponent<ScreenShake>();
        if (screenShake != null)
        {
            float impactMagnitude = Mathf.Max(gun.impactMagnitude, 0f);
            float impactDuration = Mathf.Max(gun.impactDuration, 0f);
            StartCoroutine(screenShake.Shake(impactMagnitude, impactDuration));
        }
    }
}
