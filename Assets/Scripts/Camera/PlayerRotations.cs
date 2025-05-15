using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;
using Unity.Netcode;
public class PlayerRotations : MonoBehaviour
{
    [Header("Player Settings")]
    public float sensitivity = 20f;

    [Header("References")]
    [SerializeField] private Inputs inputs;
    [SerializeField] private Player player;

    private float rotationX = 0f;
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Start()
    {

        if (IsOnlineMode)
        {
            if (!player.IsOwner) // If this is not the local player
            {
                Camera camera = GetComponent<Camera>();
                if (camera != null)
                {
                    camera.enabled = false;
                }
                return;
            }
        }


        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for PC
            Cursor.visible = false;
        }
    }


    void Update()
    {
        if (IsOnlineMode)
        {
            if (!player.IsOwner) return;
            HandleScreenRotation();
        }
        else
        {
            HandleScreenRotation();
        }

    }

    private void HandleScreenRotation()
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
                    lookInput = touch.delta.ReadValue();
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
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        // Rotate Camera (Up/Down)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotate Player (Left/Right)
        player.transform.Rotate(Vector3.up * mouseX);
    }
}
