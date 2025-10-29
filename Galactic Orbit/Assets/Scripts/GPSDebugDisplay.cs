using UnityEngine;
using TMPro;

public class GPSDebugDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI debugText;
    public GameObject debugPanel; // Optional: panel background
    
    [Header("Character Reference")]
    public Transform characterTransform; // Your character to track movement
    
    [Header("Display Settings")]
    public bool showDebug = true;
    public float updateInterval = 0.5f; // Update twice per second
    
    private float lastUpdateTime;
    private Vector3 lastCharacterPosition;
    private float totalDistanceMoved = 0f;
    private float currentSpeed = 0f;
    private Vector3 lastGPSPosition;
    private bool hasInitialized = false;
    
    void Start()
    {
        if (characterTransform != null)
        {
            lastCharacterPosition = characterTransform.position;
        }
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(showDebug);
        }
        
        lastUpdateTime = Time.time;
    }
    
    void Update()
    {
        if (!showDebug || GPSManager.Instance == null) return;
        
        // Update display at intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDebugDisplay();
            lastUpdateTime = Time.time;
        }
        
        // Track character movement
        if (characterTransform != null)
        {
            float distanceThisFrame = Vector3.Distance(characterTransform.position, lastCharacterPosition);
            totalDistanceMoved += distanceThisFrame;
            
            // Calculate speed (units per second)
            currentSpeed = distanceThisFrame / Time.deltaTime;
            
            lastCharacterPosition = characterTransform.position;
        }
    }
    
    void UpdateDebugDisplay()
    {
        if (debugText == null || GPSManager.Instance == null) return;
        
        // Check if GPS is initialized
        if (!hasInitialized && GPSManager.Instance.latitude != 0)
        {
            hasInitialized = true;
            lastGPSPosition = new Vector3(
                GPSManager.Instance.latitude,
                0,
                GPSManager.Instance.longitude
            );
        }
        
        // Get GPS status
        string gpsStatus = GetGPSStatus();
        
        // Calculate GPS movement
        Vector3 currentGPSPosition = new Vector3(
            GPSManager.Instance.latitude,
            0,
            GPSManager.Instance.longitude
        );
        
        float gpsMovement = hasInitialized ? 
            Vector3.Distance(currentGPSPosition, lastGPSPosition) : 0f;
        
        // Convert GPS degrees to approximate meters
        float metersPerDegree = 111320f; // Approximate
        float gpsMetersMovement = gpsMovement * metersPerDegree;
        
        // Build debug text
        string debugInfo = "=== GPS DEBUG ===\n\n";
        
        debugInfo += $"<b>GPS STATUS:</b> {gpsStatus}\n\n";
        
        debugInfo += $"<b>GPS COORDINATES:</b>\n";
        debugInfo += $"Lat: {GPSManager.Instance.latitude:F6}\n";
        debugInfo += $"Lon: {GPSManager.Instance.longitude:F6}\n";        
        if (characterTransform != null)
        {
            debugInfo += $"<b>CHARACTER POSITION:</b>\n";
            debugInfo += $"X: {characterTransform.position.x:F2}\n";
            debugInfo += $"Y: {characterTransform.position.y:F2}\n";
            debugInfo += $"Z: {characterTransform.position.z:F2}\n\n";
            
            debugInfo += $"<b>MOVEMENT:</b>\n";
            debugInfo += $"Total Distance: {totalDistanceMoved:F2}m\n";
            debugInfo += $"Current Speed: {currentSpeed:F2} m/s\n";
            debugInfo += $"GPS Movement: {gpsMetersMovement:F2}m\n\n";
        }
        
        debugInfo += $"<b>LOCATION SERVICE:</b>\n";
        debugInfo += $"Status: {Input.location.status}\n";
        debugInfo += $"Enabled: {Input.location.isEnabledByUser}\n";
        
        debugText.text = debugInfo;
        
        lastGPSPosition = currentGPSPosition;
    }
    
    string GetGPSStatus()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            if (GPSManager.Instance.latitude != 0 && GPSManager.Instance.longitude != 0)
                return "<color=green>● ACTIVE</color>";
            else
                return "<color=yellow>● INITIALIZING</color>";
        }
        else if (Input.location.status == LocationServiceStatus.Initializing)
        {
            return "<color=yellow>● STARTING...</color>";
        }
        else if (Input.location.status == LocationServiceStatus.Stopped)
        {
            return "<color=red>● STOPPED</color>";
        }
        else
        {
            return "<color=red>● FAILED</color>";
        }
    }
    
    // Call this to toggle debug display on/off
    public void ToggleDebug()
    {
        showDebug = !showDebug;
        if (debugPanel != null)
        {
            debugPanel.SetActive(showDebug);
        }
    }
    
    // Call this to reset stats
    public void ResetStats()
    {
        totalDistanceMoved = 0f;
        currentSpeed = 0f;
        if (characterTransform != null)
        {
            lastCharacterPosition = characterTransform.position;
        }
    }
}