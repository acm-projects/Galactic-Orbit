using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System;

/// <summary>
/// Keeps the player fixed at the center,
/// but only recenters the map once the player moves beyond a chunk distance.
/// Also calculates velocity based on irregular GPS updates.
/// </summary>
public class MapFollowerSmooth : MonoBehaviour
{
    [Header("References")]
    public AbstractMap map;
    public Transform player;

    [Header("Chunk Settings")]
    [Tooltip("Distance (in meters) before the map recenters.")]
    //public float recenterThreshold = 25f;

    [Header("Smooth Settings")]
    public float recenterSpeed = 3f;

    public event Action<Vector3> OnVelocityCalculated;  // ✅ velocity broadcast

    private Vector2d _mapCenter;
    private Vector2d _targetLocation;
    private bool _isRecentering;

    // For velocity calculation
    private Vector2d _lastTargetPos;
    private float _lastUpdateTime;
    public float velocityDeadZone = 0.05f; // meters — tweak to your liking

    public Vector2d currentLocation;
    void OnEnable()
    {
        InitializeMapFollower();
    }

    void Start()
    {
        // Optional: you can remove Start entirely
        InitializeMapFollower();
    }

    private void InitializeMapFollower()
    {
        // Reset flags
        _isRecentering = false;

        // Ensure location is loaded
        InitializeLocation();

        _mapCenter = currentLocation;
        _targetLocation = _mapCenter;

        // Reinitialize map ONLY if map exists
        if (map != null)
            map.Initialize(_mapCenter, (int)map.Zoom);

        // Reset player position
        if (player != null)
            player.position = Vector3.zero;

        // Reset velocity system
        _lastTargetPos = _targetLocation;
        _lastUpdateTime = Time.time;
}

    public void InitializeLocation()
    {
        if (GPSManager.Instance != null)
        {
            
            //get distance from last location
            var distance = GPSManager.Instance.GetMilesDistanceFromLocation(currentLocation);
            bool addDistance = true;
            if (distance < 0.00000001)
                addDistance = false;
            if (distance > 100)
            {
                Debug.Log($"Distance {distance} miles is too large to travel");
                addDistance = false; // traveling 10 miles in 3 seconds??
            }
            if (UserProfileManager.Instance != null && addDistance)
            {
                UserProfileManager.Instance.AddDistance(distance, (success, msg) => 
                {
                    if (success)
                    {
                        Debug.Log($"✅ {msg}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to add distance: {msg}");
                    }
                });
            }
           /* else
            {
                Debug.LogWarning("UserProfileManager not found! distance not added.");
            }*/
            currentLocation = new Vector2d(GPSManager.Instance.latitude, GPSManager.Instance.longitude);
        }
    }

    void Update()
    {
        // Simulate receiving new GPS data at irregular intervals
        // (In a real game, this would come from a GPS manager or network event)
        if (!_isRecentering) // every ~2 seconds
        {
            InitializeLocation();
            Vector2d newGps = currentLocation;
            ReceiveNewLocation(newGps);
            
        }

        // Keep player at center
        if (player != null)
            player.position = Vector3.zero;
    }

    /// <summary>
    /// Called whenever new GPS coordinates are received.
    /// </summary>
    public void ReceiveNewLocation(Vector2d newLocation)
    {
        // Convert GPS to world position
        Vector3 worldPos = Conversions.GeoToWorldPosition(
            newLocation,
            map.CenterMercator,
            map.WorldRelativeScale).ToVector3xz();
        Vector3 lastWorldPos = Conversions.GeoToWorldPosition(
            _targetLocation,
            map.CenterMercator,
            map.WorldRelativeScale).ToVector3xz();

        // --- Velocity Calculation ---
        float deltaTime = Time.time - _lastUpdateTime;
        if (deltaTime > 0f)
        {

            Vector3 displacement = worldPos - lastWorldPos;
            float distance = displacement.magnitude;

            //Debug.Log($"Displacement: {displacement}, Distance: {distance}, DeltaTime: {deltaTime}");

            if (distance < velocityDeadZone)
            {
                // Ignore jitter — treat as no movement
                OnVelocityCalculated?.Invoke(Vector3.zero);
            }
            else
            {
                Vector3 velocity = displacement / deltaTime;
                OnVelocityCalculated?.Invoke(velocity);
                //float distanceFromCenter = worldPos.magnitude;
            }
            if (!_isRecentering)
                {
                    StartCoroutine(RecenterMap(newLocation, Time.time - _lastUpdateTime));
                }
        }
        

        _lastTargetPos = _targetLocation;
        _lastUpdateTime = Time.time;
        _targetLocation = newLocation;

        // --- Recenter if outside chunk ---
    }

    private System.Collections.IEnumerator RecenterMap(Vector2d newCenter, float duration = 1f)
    {
        _isRecentering = true;
        Vector2d start = _mapCenter;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            _mapCenter = LerpGeo(start, newCenter, t);
            map.UpdateMap(_mapCenter);

            yield return null;  // wait for next frame
        }

        // ✅ Finalize new center
        _mapCenter = newCenter;
        _targetLocation = newCenter;

        // ✅ IMPORTANT STEP:
        // Recalculate _lastWorldPos in the NEW coordinate frame
        /*_lastWorldPos = Conversions.GeoToWorldPosition(
            _targetLocation,                 // use the last known GPS location
            map.CenterMercator,
            map.WorldRelativeScale).ToVector3xz();*/

        _isRecentering = false;
    }


    private Vector2d LerpGeo(Vector2d a, Vector2d b, float t)
    {
        double lat = Mathf.Lerp((float)a.x, (float)b.x, t);
        double lon = Mathf.Lerp((float)a.y, (float)b.y, t);
        return new Vector2d(lat, lon);
    }
}
