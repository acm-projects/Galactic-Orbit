using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class CameraOrbitNewInput : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 0.2f;

    private Vector2 previousPosition;
    private bool isDragging = false;

    void Update()
    {
        // --- TOUCH INPUT ---
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();

            if (!isDragging)
            {
                previousPosition = touchPos;
                isDragging = true;
            }
            else
            {
                Vector2 delta = touchPos - previousPosition;
                previousPosition = touchPos;
                RotateCamera(delta);
            }
        }
        else
        {
            isDragging = false;
        }

        // --- MOUSE INPUT ---
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!isDragging)
            {
                previousPosition = mousePos;
                isDragging = true;
            }
            else
            {
                Vector2 delta = mousePos - previousPosition;
                previousPosition = mousePos;
                RotateCamera(delta);
            }
        }
        else
        {
            isDragging = false;
        }
    }

    void RotateCamera(Vector2 delta)
    {
        transform.RotateAround(target.position, Vector3.up, delta.x * rotationSpeed);
        transform.RotateAround(target.position, transform.right, -delta.y * rotationSpeed);
    }
}
