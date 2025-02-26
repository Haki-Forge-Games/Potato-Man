using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private Inputs inputs;


    void Update()
    {
        if (inputs == null) return;


        Vector2 movement = inputs.GetMovementVectorTwo();
        Vector3 moveDir = (transform.forward * movement.y) + (transform.right * movement.x);
        transform.position += moveDir * Time.deltaTime * movementSpeed;
    }
}
