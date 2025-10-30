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
            col.size = new Vector3(1.2f, 1.2f, 1.2f); // Slightly bigger for easier tapping
        }
        
        Debug.Log($"AR Collectible spawned: {itemName}");
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // rotate continuously
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // bob up and down animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        CheckForTap();
    }
    
    void CheckForTap()
    {
        bool tapped = false;
        Vector2 screenPos = Vector2.zero;
        
        // touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // mouse input (testing in editor)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Mouse.current.position.ReadValue();
        }
        
        if (tapped)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 50f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Collect();
                }
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