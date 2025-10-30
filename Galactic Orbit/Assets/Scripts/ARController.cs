using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class SimpleARController : MonoBehaviour
{
    [Header("Camera References")]
    public GameObject normalCamera; // the regular game (map view) camera
    public GameObject arSessionOrigin; // XR Origin (Mobile AR)
    public GameObject arSession; // AR Session GameObject
    public GameObject MainMenuOObject; // Reference to the main UI Document
    
    [Header("AR Objects to Spawn")]
    public GameObject arObjectPrefab; // The object that appears in AR
    public float spawnDistance = 2f; // How far from player (min)
    public float spawnDistanceMax = 4f; // How far from player (max)
    public float spawnHeight = 0f; // Height above ground (0 = ground level)
    
    [Header("AR Components (Auto-assigned)")]
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private Camera arCamera;
    
    private bool isARMode = false;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    void Start()
    {
        // Get AR components from XR Origin
        if (arSessionOrigin != null)
        {
            raycastManager = arSessionOrigin.GetComponent<ARRaycastManager>();
            planeManager = arSessionOrigin.GetComponent<ARPlaneManager>();
            arCamera = arSessionOrigin.GetComponentInChildren<Camera>();
        }
        
        SetARMode(false);
        
        Debug.Log("SimpleARController initialized");
    }
    
    void Update()
    {
        if (!isARMode) return;
        
        // Android back button closes AR
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseARMode();
        }
    }
    
    // for situations when you want a set on/off ar button (isntead of current toggle)
    public void ToggleARMode()
    {
        SetARMode(!isARMode);
        // toggle MainMenu UI
        if (MainMenuOObject != null)
        {
            MainMenuOObject.SetActive(!isARMode);
        }
    }
    
    public void OpenARMode()
    {
        Debug.Log("=== OPENING AR MODE ===");
        SetARMode(true);
    }
    
    public void CloseARMode()
    {
        Debug.Log("=== CLOSING AR MODE ===");
        SetARMode(false);
    }
    
    void SetARMode(bool enabled)
    {
        isARMode = enabled;
        
        // Switch cameras
        if (normalCamera != null)
        {
            normalCamera.SetActive(!enabled);
            Debug.Log($"Normal Camera: {!enabled}");
        }
        
        if (arSessionOrigin != null)
        {
            arSessionOrigin.SetActive(enabled);
            Debug.Log($"XR Origin: {enabled}");
        }
        
        if (arSession != null)
        {
            arSession.SetActive(enabled);
            Debug.Log($"AR Session: {enabled}");
        }
        
        Debug.Log(enabled ? "AR Mode ENABLED - Point camera at surfaces" : "AR Mode DISABLED");
        
        // spawn object when entering AR
        if (enabled)
        {
            // Wait a moment for AR to initialize, then spawn
            Invoke("SpawnRandomObject", 0.5f);
        }
        else
        {
            ClearSpawnedObjects();
        }
    }
    
    void HandleARTap()
    {
        // check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            TrySpawnObject(touchPosition);
        }
        // Mouse for testing
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            TrySpawnObject(mousePosition);
        }
    }
    
    void TrySpawnObject(Vector2 screenPosition)
    {
        if (arObjectPrefab == null)
        {
            Debug.LogWarning("No AR object prefab assigned!");
            return;
        }
        
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        if (raycastManager != null && raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // spawn on detected surface
            Pose hitPose = hits[0].pose;
            SpawnObjectAt(hitPose.position, hitPose.rotation);
            Debug.Log("Spawned object on AR plane");
        }
        else
        {
            // spawn in front of camera if no surface detected
            Debug.Log("no surface detected");
            SpawnInFrontOfCamera();
        }
    }
    
    void SpawnInFrontOfCamera()
    {
        if (arCamera == null || arObjectPrefab == null) return;
        
        Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.forward * spawnDistance;
        SpawnObjectAt(spawnPosition, Quaternion.identity);
    }
    
    void SpawnRandomObject()
    {
        if (arObjectPrefab == null || arCamera == null)
        {
            Debug.LogWarning("canoot spawn");
            return;
        }
        
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(spawnDistance, spawnDistanceMax);
        
        // calculate position in circle around camera
        Vector3 directionFromCamera = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        Vector3 spawnPosition = arCamera.transform.position + directionFromCamera * randomDistance;
        
        // set height (relative to camera height)
        spawnPosition.y = arCamera.transform.position.y + spawnHeight;
        
        // Try to raycast down to find floor
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
        {
            // spawn on floor
            spawnPosition = hit.point + Vector3.up * 0.1f;
            Debug.Log($"Spawned on floor at {spawnPosition}");
        }
        else
        {
            Debug.Log($"Spawned at fixed height: {spawnPosition}");
        }
        
        SpawnObjectAt(spawnPosition, Quaternion.identity);
    }
    
    void SpawnObjectAt(Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(arObjectPrefab, position, rotation);
        spawnedObjects.Add(spawnedObject);
        
        Debug.Log($"Spawned object at {position}");
    }

    void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
        Debug.Log("Cleared all spawned AR objects");
    }

    void OnGUI()
{
    if (!isARMode) return;
    
    GUIStyle style = new GUIStyle();
    style.fontSize = 30;
    style.normal.textColor = Color.yellow;
    
    string text = $"AR MODE ACTIVE\n";
    text += $"Spawned Objects: {spawnedObjects.Count}\n";
    text += $"Camera: {(arCamera != null ? "OK" : "NULL")}\n";
    text += $"Prefab: {(arObjectPrefab != null ? "OK" : "NULL")}\n";
    
    if (arCamera != null)
    {
        text += $"Cam Pos: {arCamera.transform.position}\n";
    }
    
    GUI.Label(new Rect(10, 100, 600, 300), text, style);
}
}