using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Orbit Settings")]
    public float distance = 10f;
    public float height = 2f;
    public float rotationSpeed = 100f;
    
    [Header("Touch Controls")]
    public float touchSensitivity = 0.5f;
    public bool invertRotation = false;
    
    [Header("Tilt Settings")]
    public float minTiltAngle = 10f;
    public float maxTiltAngle = 80f;
    
    private float currentAngle = 0f;
    private float currentTilt = 30f;
    private bool isDragging = false;
    private Vector2 lastInputPosition;
    
    void Start()
    {
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        // touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (!isDragging)
            {
                isDragging = true;
                lastInputPosition = touchPosition;
            }
            else
            {
                Vector2 delta = touchPosition - lastInputPosition;
                RotateCamera(delta.x, delta.y);
                lastInputPosition = touchPosition;
            }
        }
        // mouse input (editor testing)
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            
            if (!isDragging)
            {
                isDragging = true;
                lastInputPosition = mousePosition;
            }
            else
            {
                Vector2 delta = mousePosition - lastInputPosition;
                RotateCamera(delta.x, delta.y);
                lastInputPosition = mousePosition;
            }
        }
        else
        {
            isDragging = false;
        }
    }
    
    void RotateCamera(float horizontalDelta, float verticalDelta)
    {
        // horizontal rotation around character
        float rotationAmount = horizontalDelta * touchSensitivity;
        if (invertRotation) rotationAmount = -rotationAmount;
        currentAngle += rotationAmount;
        
        // vertical tilt
        currentTilt -= verticalDelta * touchSensitivity * 0.1f;
        currentTilt = Mathf.Clamp(currentTilt, minTiltAngle, maxTiltAngle);
    }
    
    void UpdateCameraPosition()
    {
        float radians = currentAngle * Mathf.Deg2Rad;
        float tiltRadians = currentTilt * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(radians) * Mathf.Cos(tiltRadians) * distance,
            Mathf.Sin(tiltRadians) * distance + height,
            Mathf.Cos(radians) * Mathf.Cos(tiltRadians) * distance
        );
        
        transform.position = target.position + offset;
        
        transform.LookAt(target.position + Vector3.up * height);
    }
    
    public void ResetCamera()
    {
        currentAngle = 0f;
        currentTilt = 30f;
    }
}