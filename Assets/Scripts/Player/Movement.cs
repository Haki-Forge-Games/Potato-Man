using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Movement : NetworkBehaviour
{
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;
    [SerializeField] private Animator animator;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    void Update()
    {
        if (inputs == null) return;
        if (IsOnlineMode && IsOwner) HandleMovement();
        if (!IsOnlineMode) HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 movement = inputs.GetMovementVectorTwo();
        Vector3 moveDir = (transform.forward * movement.y) + (transform.right * movement.x);

        if (moveDir.sqrMagnitude > 0)
        {
            animator.SetBool("IsWalk", true);
        }
        else
        {
            animator.SetBool("IsWalk", false);
        }

        transform.position += moveDir * Time.deltaTime * player.movementSpeed;
    }
}
