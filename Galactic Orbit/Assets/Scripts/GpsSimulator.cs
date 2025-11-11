using UnityEngine;
using UnityEngine.InputSystem;

public class GPSSimulator : MonoBehaviour
{
    [Header("Simulated GPS")]
    public bool useSimulatedGPS = true;
    public float simulatedLatitude = 0f; 
    public float simulatedLongitude = 0f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 0.0000001f; 
    public float fastMoveMultiplier = 3f; 
    
    [Header("Display")]
    public bool showDebugInfo = true;
    
    private float totalDistanceMoved = 0f;
    private Vector2 lastPosition;
    
    void Start()
    {
        lastPosition = new Vector2(simulatedLatitude, simulatedLongitude);
    }
    
    void Update()
    {
        if (!useSimulatedGPS) return;
        
        float currentSpeed = moveSpeed;
        
        // faster movement with Shift
        if (Keyboard.current != null && Keyboard.current.shiftKey.isPressed)
        {
            currentSpeed *= fastMoveMultiplier;
        }
        
        // WASD to simulate GPS movement
        if (Keyboard.current != null)
        {
            Vector2 oldPos = new Vector2(simulatedLatitude, simulatedLongitude);
            
            // W/S = North/South (latitude)
            if (Keyboard.current.wKey.isPressed)
                simulatedLatitude += currentSpeed;
            if (Keyboard.current.sKey.isPressed)
                simulatedLatitude -= currentSpeed;
            
            // A/D = West/East (longitude)
            if (Keyboard.current.aKey.isPressed)
                simulatedLongitude -= currentSpeed;
            if (Keyboard.current.dKey.isPressed)
                simulatedLongitude += currentSpeed;
            
            Vector2 newPos = new Vector2(simulatedLatitude, simulatedLongitude);
            totalDistanceMoved += Vector2.Distance(oldPos, newPos);
        }
        
        // send simulated GPS to GPSManager
        if (GPSManager.Instance != null)
        {
            GPSManager.Instance.latitude = simulatedLatitude;
            GPSManager.Instance.longitude = simulatedLongitude;
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || !useSimulatedGPS) return;
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.white;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        
        string speedText = (Keyboard.current != null && Keyboard.current.shiftKey.isPressed) ? "FAST" : "Normal";
        
        string debugText = $"=== GPS SIMULATOR ===\n" +
                          $"Lat: {simulatedLatitude:F6}\n" +
                          $"Lon: {simulatedLongitude:F6}\n" +
                          $"Distance: {totalDistanceMoved:F4}\n" +
                          $"\n[WASD] Move\n" +
                          $"[Shift] {speedText}\n" +
                          $"[R] Reset to 0,0";
        
        GUI.Box(new Rect(10, 10, 280, 180), debugText, boxStyle);
        
        // reset button
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetPosition();
        }
    }
    
    void ResetPosition()
    {
        simulatedLatitude = 0f;
        simulatedLongitude = 0f;
        totalDistanceMoved = 0f;
        Debug.Log("GPS position reset to 0,0");
    }
    
    public float GetDistanceMovedInMeters()
    {
        if (GPSManager.Instance == null) return 0f;
        
        float latDiff = simulatedLatitude;
        float lonDiff = simulatedLongitude;
        
        float metersLat = latDiff * 111320f;
        float metersLon = lonDiff * 111320f * Mathf.Cos(simulatedLatitude * Mathf.Deg2Rad);
        
        return Mathf.Sqrt(metersLat * metersLat + metersLon * metersLon);
    }
}