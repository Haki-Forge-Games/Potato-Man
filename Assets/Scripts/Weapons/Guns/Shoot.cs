using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Shoot : NetworkBehaviour
{
    [Header("Shoot Settings")]
    [SerializeField] private float shootRange = 10f;
    [SerializeField] private float fireRate = 0.2f;


    [Header("References")]
    [SerializeField] private Inputs input;
    [SerializeField] private Item item;
    [SerializeField] private ParticleSystem muzzleFlash;

    private float lastShotTime = 0f;
    private RaycastHit hitInfo;


    public NetworkVariable<int> currentBullets { get; set; } = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Update()
    {
        Debug.Log(currentBullets.Value);
        if (!IsValidPlayer() || !CanShoot()) return;
        ShootGun();
        lastShotTime = Time.time;
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
    // Checks if the player can shoot.
    // </summary>
    private bool CanShoot()
    {
        if (input != null && item != null)
        {
            if (item.isPickedUp && input.CheckShootPressed())
                if (Time.time > lastShotTime + fireRate) return true;
        }

        return false;
    }

    private void ShootGun()
    {
        if (currentBullets.Value <= 0)
        {
            Debug.Log("Gun is empty");
            return;
        }

        DecrementBullets(1); // Decrement the bullets by 1
        PlayMuzzleFlash(); // Play the muzzle flash effect

        if (IsEnemyHit())
            HandleEnemyHit(hitInfo.collider?.gameObject);
    }

    // <summary>
    // Checks if the raycast hit an enemy.
    // </summary>
    // <returns>True if an enemy was hit, false otherwise.</returns>
    private bool IsEnemyHit()
    {
        if (item?.owner?.camera == null) return false; // Ensure the camera is not null
        Camera camera = item.owner.camera;

        // Perform a raycast in the direction camera is facing and checks if it hits an enemy
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo, shootRange) &&
               hitInfo.collider.CompareTag("Enemy");
    }

    // <summary>
    // Plays the muzzle flash effect.
    // </summary>
    private void PlayMuzzleFlash()
    {
        if (muzzleFlash == null) return;
        muzzleFlash.Play();
    }

    // <summary>
    // Incremen the number of bullets in the gun.
    // </summary>
    public void IncrementBullets(int value)
    {
        currentBullets.Value += value;
    }

    // <summary>
    // Decrements the number of bullets in the gun.
    // </summary>
    private void DecrementBullets(int value)
    {
        currentBullets.Value -= value;
    }

    // <summary>
    // Handles the enemy hit by changing its state to death.
    // </summary>
    private void HandleEnemyHit(GameObject enemy)
    {
        if (enemy == null) return;

        StateManager stateManager = enemy.GetComponent<StateManager>();
        if (stateManager == null) return;

        if (IsOnlineMode)
            ChangeEnemyStateServerRpc(enemy.GetComponent<NetworkObject>());
        else
            stateManager.ChangeStateToDeath();
    }

    // <summary>
    // Change enemy state to death on server.
    // </summary>
    [ServerRpc]
    public void ChangeEnemyStateServerRpc(NetworkObjectReference enemyRef)
    {
        if (!enemyRef.TryGet(out NetworkObject ntwObject)) return;
        StateManager controller = ntwObject.gameObject.GetComponent<StateManager>();

        if (controller != null)
        {
            controller.ChangeStateToDeath();
            ChangeEnemyStateClientRpc(enemyRef);
        }
    }

    // <summary>
    // Reflect enemy state change to all clients.
    // </summary>
    [ClientRpc]
    private void ChangeEnemyStateClientRpc(NetworkObjectReference enemyRef)
    {
        if (!enemyRef.TryGet(out NetworkObject ntwObject)) return;
        StateManager controller = ntwObject.gameObject.GetComponent<StateManager>();
        controller?.ChangeStateToDeath();
    }
}
