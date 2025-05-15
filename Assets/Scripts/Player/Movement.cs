using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Movement : NetworkBehaviour
{
    [Header("Player Settings")]
    public float movementSpeed = 10f;

    [Header("References")]
    [SerializeField] private Inputs inputs;
    [SerializeField] private Animator animator;


    public bool IsWalking { get; set; } = false;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    void Update()
    {
        if (IsOnlineMode && !IsOwner) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (inputs == null) return;
        Vector2 movement = inputs.GetMovementVectorTwo();
        Vector3 moveDir = (transform.forward * movement.y) + (transform.right * movement.x);

        if (moveDir.sqrMagnitude > 0)
        {
            animator.SetBool("IsWalk", true);
            IsWalking = true;
        }
        else
        {
            animator.SetBool("IsWalk", false);
            IsWalking = false;
        }

        transform.position += moveDir * Time.deltaTime * movementSpeed;
    }
}
