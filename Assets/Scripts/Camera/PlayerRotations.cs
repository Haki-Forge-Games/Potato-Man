using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;

public class PlayerRotations : MonoBehaviour
{
    public Transform playerBody;
    public float Sensitivity = 100f;

    private float rotationX = 0f;
    [SerializeField] private Inputs inputs;

    void Update()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            if (touchPos.x < Screen.width / 2) return; // Ignore left side touches
        }

        Vector2 lookInput = inputs.GetScreenRotationVeectorTwo();

        float mouseX = lookInput.x * Sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * Sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
