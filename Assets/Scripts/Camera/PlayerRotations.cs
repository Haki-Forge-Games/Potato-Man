using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;

public class PlayerRotations : MonoBehaviour
{
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;

    private float rotationX = 0f;

    private void Start()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for PC
            Cursor.visible = false;
        }
    }


    void Update()
    {
        Vector2 lookInput = Vector2.zero;

        // for mobile
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed) continue; // Skip if touch is not active

                Vector2 touchPos = touch.position.ReadValue();

                // Check if touch is on the right side of the screen
                if (touchPos.x > Screen.width / 2)
                {
                    lookInput = touch.delta.ReadValue() * player.Sensitivity;
                    break; // Stop after finding the first valid right-side touch
                }
            }
        }
        else
        {
            // for pc 
            lookInput = inputs.GetScreenRotationVeectorTwo();
        }

        CameraLook(lookInput);
    }

    private void CameraLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * player.Sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * player.Sensitivity * Time.deltaTime;

        // Rotate Camera (Up/Down)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotate Player (Left/Right)
        player.transform.Rotate(Vector3.up * mouseX);
    }
}
