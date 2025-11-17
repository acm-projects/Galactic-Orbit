using System.Collections;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using System;   // Adding the big library directly for points to be added

public class ARCollectible : MonoBehaviour
{
    public bool CanCollect = true;
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
        Vector3 rotationDirection = Vector3.up;
        transform.Rotate(rotationDirection, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        if (CanCollect)
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

        // This is where points are added, hopefully reflected in Firebase
        // Keeping 20 as a constant for now, can easily be changed later
        if (UserProfileManager.Instance != null)
        {
            UserProfileManager.Instance.AddPoints(20, (success, message) =>
            {
                if (success)
                {
                    Debug.Log($"🎉 Points awarded! {message}");
                }
                else
                {
                    Debug.LogError($"❌ Failed to award points: {message}");
                }
            });
        }
        else
        {
            Debug.LogError("❌ UserProfileManager.Instance is null - points not awarded!");
        }
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
