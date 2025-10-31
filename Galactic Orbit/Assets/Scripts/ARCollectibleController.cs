using UnityEngine;
using UnityEngine.InputSystem;

public class ARCollectible : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    
    [Header("Collection")]
    public string itemName = "Cube";
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        // add collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.02f, 0.02f, 0.02f); // Slightly bigger for easier tapping
        }
        
        Debug.Log($"AR Collectible spawned: {itemName}");
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // rotate continuously
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        
        // bob up and down animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        CheckForTap();
    }
    
    void CheckForTap()
    {
        bool tapped = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (tapped)
        {
            Debug.Log($"Tapped at {screenPos}");
            if (Camera.main == null)
            {
                Debug.LogError("No camera tagged as MainCamera!");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);
            Debug.Log(ray.direction);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                Debug.Log($"Hit: {hit.collider.name}");
                if (hit.collider.gameObject == gameObject)
                    Collect();
            }
            else
            {
                Debug.Log("No hit detected.");
            }
        }
    }


    void Collect()
    {
        if (isCollected) return;

        isCollected = true;
        Debug.Log($"✅ Collected: {itemName}!");

        // destroy with slight delay
        Destroy(gameObject, 0.2f);
    }
}