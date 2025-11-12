using System.Collections;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class ARCollectible : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;

    [Header("Collection")]
    public string itemName = "Cube";
    public float shrinkDuration = 0.4f;

    private Vector3 startPosition;
    private bool isCollected = false;
    private Camera arCamera;
    
    private bool shrinkCompleted = false;

    void Start()
    {
        startPosition = transform.position;

        // find AR camera instead of Camera.main
        var arOrigin = FindFirstObjectByType<XROrigin>();
        if (arOrigin != null)
            arCamera = arOrigin.Camera;
        else
            arCamera = Camera.main; // fallback for non-AR

        // add collider if missing
        if (GetComponent<Collider>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider>();
            col.size = Vector3.one * 2f;
        }

        Debug.Log($"AR Collectible spawned: {itemName} using camera: {arCamera?.name}");
    }

    void Update()
    {
        if (isCollected) return;

        // rotate & bob
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        CheckForTap();
    }

    async void CheckForTap()
    {
        bool tapped = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame ?? false)
        {
            tapped = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current?.leftButton.wasPressedThisFrame ?? false)
        {
            tapped = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (!tapped || arCamera == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 5f, Color.green, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Debug.Log($"Hit: {hit.collider.name}");
            if (hit.collider.gameObject == gameObject)
                // Might have issue bc CheckForTap method is void instead of Async
                await Collect();
        }
        else
        {
            Debug.Log("No hit detected.");
        }
    }

    async Task Collect()
    {
        if (isCollected) return;
        isCollected = true;
        Debug.Log($"✅ Collected: {itemName}");
        shrinkCompleted = false;
        StartCoroutine(LerpShrink()); // Shrinks until deleted
        while (!shrinkCompleted)
        {
            await Task.Yield();
        }
        Destroy(gameObject);
    }

    IEnumerator LerpShrink()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        shrinkCompleted = true;
    }
}
