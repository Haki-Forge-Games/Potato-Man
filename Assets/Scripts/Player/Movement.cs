using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;

    void Update()
    {
        if (inputs == null) return;

        Vector2 movement = inputs.GetMovementVectorTwo();
        Vector3 moveDir = (transform.forward * movement.y) + (transform.right * movement.x);
        transform.position += moveDir * Time.deltaTime * player.movementSpeed;
    }
}
