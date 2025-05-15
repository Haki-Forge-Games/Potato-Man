using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("References")]
    public Camera camera;
    public GameObject playerModel;
    public Transform holdPosition;

    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {
        if (playerModel == null || !IsOnlineMode) return;

        if (!IsOwner)
        {
            playerModel.layer = 0;
            foreach (Transform child in playerModel.transform)
            {
                child.gameObject.layer = 0;
            }
        }
    }
}
