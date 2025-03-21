using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float movementSpeed = 50f;
    public float pickUpDistance = 50f;
    public float Sensitivity = 100f;
    public Transform holdPosition;
    public GameObject playerModel;

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
