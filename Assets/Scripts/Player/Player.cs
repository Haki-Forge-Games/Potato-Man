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
}
