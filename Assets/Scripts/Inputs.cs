using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour
{
    // new Inputs system script 
    private PlayerInputs playerInputs;

    private void Awake()
    {
        playerInputs = new PlayerInputs();  // creating instance obj 
    }

    private void OnEnable()
    {
        if (playerInputs != null)
        {
            playerInputs.Enable();
        }
    }

    private void OnDisable()
    {
        if (playerInputs != null)
        {
            playerInputs.Disable();
        }
    }

    public Vector2 GetMovementVectorTwo()
    {
        if (playerInputs != null)
        {
            Vector2 moveInput = playerInputs.Player.Move.ReadValue<Vector2>();
            return moveInput;
        }
        else
        {
            return new Vector2(0f, 0f);
        }
    }

    public Vector2 GetScreenRotationVeectorTwo()
    {
        if (playerInputs != null)
        {
            Vector2 moveInput = playerInputs.Player.Look.ReadValue<Vector2>();
            return moveInput;
        }
        else
        {
            return new Vector2(0f, 0f);
        }
    }

    public bool CheckPickUpPressed()
    {
        if (playerInputs.Player.PickDrop.WasPressedThisFrame()) return true;
        return false;
    }

    public bool CheckShootPressed()
    {
        if (playerInputs.Weapons.Shoot.WasPressedThisFrame()) return true;
        return false;
    }
}
