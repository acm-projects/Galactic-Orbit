using UnityEngine;

/// <summary>
/// Checks if player is near quest/event locations using GPS coordinates
/// </summary>
public class ProximityChecker : MonoBehaviour
{
    public static ProximityChecker Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Check if player is within a certain radius of a GPS location
    /// </summary>
    /// <param name="targetLatitude">Target location latitude</param>
    /// <param name="targetLongitude">Target location longitude</param>
    /// <param name="radiusMeters">Activation radius in meters</param>
    /// <returns>True if player is within range</returns>
    public bool IsPlayerNearby(float targetLatitude, float targetLongitude, float radiusMeters)
    {
        if (GPSManager.Instance == null || !GPSManager.Instance.HasGPS)
        {
            Debug.LogWarning("GPS not available - cannot check proximity");
            return false;
        }

        float playerLat = GPSManager.Instance.latitude;
        float playerLon = GPSManager.Instance.longitude;

        float distance = CalculateDistance(playerLat, playerLon, targetLatitude, targetLongitude);

        return distance <= radiusMeters;
    }

    /// <summary>
    /// Check if player is within range of a Quest
    /// </summary>
    public bool IsPlayerNearQuest(Quest quest)
    {
        if (quest == null) return false;
        return IsPlayerNearby(quest.targetLocation.x, quest.targetLocation.y, quest.completionRadius);
    }

    /// <summary>
    /// Check if player is within range of an Event
    /// </summary>
    public bool IsPlayerNearEvent(EventData eventData)
    {
        if (eventData == null) return false;
        return IsPlayerNearby(eventData.gpsCoordinates.x, eventData.gpsCoordinates.y, eventData.activationRadius);
    }

    /// <summary>
    /// Get distance in meters between player and a GPS coordinate
    /// </summary>
    public float GetDistanceToLocation(float targetLat, float targetLon)
    {
        if (GPSManager.Instance == null || !GPSManager.Instance.HasGPS)
        {
            return float.MaxValue;
        }

        return CalculateDistance(
            GPSManager.Instance.latitude,
            GPSManager.Instance.longitude,
            targetLat,
            targetLon
        );
    }

    /// <summary>
    /// Get distance in meters between player and a Quest location
    /// </summary>
    public float GetDistanceToQuest(Quest quest)
    {
        if (quest == null) return float.MaxValue;
        return GetDistanceToLocation(quest.targetLocation.x, quest.targetLocation.y);
    }

    /// <summary>
    /// Get distance in meters between player and an Event location
    /// </summary>
    public float GetDistanceToEvent(EventData eventData)
    {
        if (eventData == null) return float.MaxValue;
        return GetDistanceToLocation(eventData.gpsCoordinates.x, eventData.gpsCoordinates.y);
    }

    /// <summary>
    /// Calculate distance between two GPS coordinates using Haversine formula
    /// (Same formula as in ARLocationManager)
    /// </summary>
    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        const float R = 6371000f; // Earth's radius in meters

        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c; // Distance in meters
    }
}