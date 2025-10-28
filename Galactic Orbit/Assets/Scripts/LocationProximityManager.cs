using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages visibility of quests and events based on player's GPS location.
/// Shows items within range (100m default), hides those outside range.
/// </summary>
public class LocationProximityManager : MonoBehaviour
{
    public static LocationProximityManager Instance { get; private set; }

    [Header("Proximity Settings")]
    [Tooltip("Distance in meters within which quests/events become visible")]
    public float visibilityRadius = 100f;

    [Header("Update Frequency")]
    [Tooltip("How often to check player location (in seconds)")]
    public float updateInterval = 5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Track which quests/events are currently visible
    private HashSet<string> visibleQuestIds = new HashSet<string>();
    private HashSet<string> visibleEventIds = new HashSet<string>();

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

    void Start()
    {
        // Wait for GPS to initialize, then start checking proximity
        if (GPSManager.Instance != null && GPSManager.Instance.HasGPS)
        {
            InvokeRepeating(nameof(UpdateProximity), 2f, updateInterval);
            Debug.Log("✅ LocationProximityManager started");
        }
        else
        {
            Debug.LogWarning("⚠️ GPS not available - proximity detection disabled");
        }
    }

    /// <summary>
    /// Main update loop - checks all quests and events for proximity
    /// </summary>
    void UpdateProximity()
    {
        if (GPSManager.Instance == null || !GPSManager.Instance.HasGPS)
            return;

        Vector2 playerPos = new Vector2(GPSManager.Instance.latitude, GPSManager.Instance.longitude);

        if (showDebugLogs)
            Debug.Log($"📍 Player at: ({playerPos.x}, {playerPos.y})");

        UpdateQuestProximity(playerPos);
        UpdateEventProximity(playerPos);
    }

    /// <summary>
    /// Check all quests and show/hide based on distance
    /// </summary>
    void UpdateQuestProximity(Vector2 playerPos)
    {
        if (QuestManager.Instance == null) return;

        foreach (Quest quest in QuestManager.Instance.allQuests)
        {
            float distance = GetDistanceMeters(playerPos, quest.targetLocation);
            bool isWithinRange = distance <= visibilityRadius;
            bool wasVisible = visibleQuestIds.Contains(quest.questID);

            // Quest just came into range
            if (isWithinRange && !wasVisible)
            {
                ShowQuest(quest, distance);
                visibleQuestIds.Add(quest.questID);
            }
            // Quest just left range
            else if (!isWithinRange && wasVisible)
            {
                HideQuest(quest, distance);
                visibleQuestIds.Remove(quest.questID);
            }
            // Quest remains in range - update distance
            else if (isWithinRange && showDebugLogs)
            {
                Debug.Log($"📍 Quest '{quest.questTitle}' - {distance:F1}m away");
            }
        }
    }

    /// <summary>
    /// Check all events and show/hide based on distance
    /// </summary>
    void UpdateEventProximity(Vector2 playerPos)
    {
        if (EventManager.Instance == null) return;

        foreach (EventData eventData in EventManager.Instance.allEvents)
        {
            float distance = GetDistanceMeters(playerPos, eventData.location);
            bool isWithinRange = distance <= visibilityRadius;
            bool wasVisible = visibleEventIds.Contains(eventData.eventID);

            // Event just came into range
            if (isWithinRange && !wasVisible)
            {
                ShowEvent(eventData, distance);
                visibleEventIds.Add(eventData.eventID);
            }
            // Event just left range
            else if (!isWithinRange && wasVisible)
            {
                HideEvent(eventData, distance);
                visibleEventIds.Remove(eventData.eventID);
            }
            // Event remains in range - update distance
            else if (isWithinRange && showDebugLogs)
            {
                Debug.Log($"📍 Event '{eventData.eventName}' - {distance:F1}m away");
            }
        }
    }

    /// <summary>
    /// Called when a quest enters visibility range
    /// </summary>
    void ShowQuest(Quest quest, float distance)
    {
        Debug.Log($"✨ Quest APPEARED: '{quest.questTitle}' ({distance:F1}m away)");
        
        // You can add UI notifications here, like:
        // NotificationManager.Instance?.ShowMessage($"New quest nearby: {quest.questTitle}");
        
        // Or play a sound effect:
        // AudioManager.Instance?.PlaySound("quest_discovered");
    }

    /// <summary>
    /// Called when a quest leaves visibility range
    /// </summary>
    void HideQuest(Quest quest, float distance)
    {
        if (showDebugLogs)
            Debug.Log($"🚶 Quest OUT OF RANGE: '{quest.questTitle}' ({distance:F1}m away)");
    }

    /// <summary>
    /// Called when an event enters visibility range
    /// </summary>
    void ShowEvent(EventData eventData, float distance)
    {
        Debug.Log($"✨ Event APPEARED: '{eventData.eventName}' ({distance:F1}m away)");
        
        // You can add UI notifications here
    }

    /// <summary>
    /// Called when an event leaves visibility range
    /// </summary>
    void HideEvent(EventData eventData, float distance)
    {
        if (showDebugLogs)
            Debug.Log($"🚶 Event OUT OF RANGE: '{eventData.eventName}' ({distance:F1}m away)");
    }

    /// <summary>
    /// Get all currently visible quests
    /// </summary>
    public List<Quest> GetVisibleQuests()
    {
        List<Quest> visible = new List<Quest>();
        
        if (QuestManager.Instance != null)
        {
            foreach (Quest quest in QuestManager.Instance.allQuests)
            {
                if (visibleQuestIds.Contains(quest.questID))
                    visible.Add(quest);
            }
        }
        
        return visible;
    }

    /// <summary>
    /// Get all currently visible events
    /// </summary>
    public List<EventData> GetVisibleEvents()
    {
        List<EventData> visible = new List<EventData>();
        
        if (EventManager.Instance != null)
        {
            foreach (EventData eventData in EventManager.Instance.allEvents)
            {
                if (visibleEventIds.Contains(eventData.eventID))
                    visible.Add(eventData);
            }
        }
        
        return visible;
    }

    /// <summary>
    /// Check if a specific quest is visible
    /// </summary>
    public bool IsQuestVisible(string questID)
    {
        return visibleQuestIds.Contains(questID);
    }

    /// <summary>
    /// Check if a specific event is visible
    /// </summary>
    public bool IsEventVisible(string eventID)
    {
        return visibleEventIds.Contains(eventID);
    }

    /// <summary>
    /// Force immediate proximity check (useful after GPS jump or quest added)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateProximity();
    }

    /// <summary>
    /// Calculate distance between two GPS coordinates using Haversine formula
    /// </summary>
    float GetDistanceMeters(Vector2 a, Vector2 b)
    {
        const float R = 6371000f; // Earth's radius in meters
        
        float dLat = Mathf.Deg2Rad * (b.x - a.x);
        float dLon = Mathf.Deg2Rad * (b.y - a.y);
        float lat1 = Mathf.Deg2Rad * a.x;
        float lat2 = Mathf.Deg2Rad * b.x;

        float h = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1) * Mathf.Cos(lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        return R * 2 * Mathf.Atan2(Mathf.Sqrt(h), Mathf.Sqrt(1 - h));
    }
}