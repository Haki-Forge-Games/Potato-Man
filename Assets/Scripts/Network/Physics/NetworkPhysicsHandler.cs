using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetworkPhysicsHandler : MonoBehaviour
{
    private NetworkRigidbody networkRb;
    private Rigidbody rb;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        if (IsOnlineMode) return;

        networkRb = GetComponent<NetworkRigidbody>();
        rb = GetComponent<Rigidbody>();

        if (!NetworkManager.Singleton.IsListening) // Offline Mode
        {
            if (networkRb) networkRb.enabled = false; // Disable Network Rigidbody
            if (rb) rb.isKinematic = false; // Enable physics
        }
    }
}
