using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitNewInput : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float rotationSpeed = 0.2f;

    [Header("Vertical Clamp (X rotation)")]
    public float minVerticalAngle = -30f;   // Look up limit (negative = up)
    public float maxVerticalAngle = 60f;    // Look down limit

    private Vector2 previousPosition;
    private bool isDragging = false;

    private float currentVerticalAngle = 0f; // Tracks X rotation (pitch)
    private float currentHorizontalAngle = 0f; // Optional: track Y rotation if you want too

    void Update()
    {
        Vector2 currentPos;
        bool pressed;

        // Touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            currentPos = Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }
        // Mouse input
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            currentPos = Mouse.current.position.ReadValue();
            pressed = true;
        }
        else
        {
            pressed = false;
            currentPos = Vector2.zero;
        }

        if (pressed)
        {
            if (!isDragging)
            {
                previousPosition = currentPos;
                isDragging = true;
            }
            else
            {
                Vector2 delta = currentPos - previousPosition;
                previousPosition = currentPos;
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
        if (target == null) return;

        // --- Horizontal (Y) rotation ---
        float horizontalAngleDelta = delta.x * rotationSpeed;
        transform.RotateAround(target.position, Vector3.up, horizontalAngleDelta);
        currentHorizontalAngle += horizontalAngleDelta;

        // --- Vertical (X) rotation with clamp ---
        float verticalAngleDelta = -delta.y * rotationSpeed;
        float newVerticalAngle = Mathf.Clamp(currentVerticalAngle + verticalAngleDelta, minVerticalAngle, maxVerticalAngle);

        // Apply only the difference after clamping
        float allowedDelta = newVerticalAngle - currentVerticalAngle;
        transform.RotateAround(target.position, transform.right, allowedDelta);
        currentVerticalAngle = newVerticalAngle;
    }
}
