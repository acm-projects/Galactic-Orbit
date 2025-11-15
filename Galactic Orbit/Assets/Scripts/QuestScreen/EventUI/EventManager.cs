using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public List<EventData> allEvents;
    public EventData selectedEvent { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public List<EventData> GetUniqueRandomEvents(int count)
    {
        
        if (count >= allEvents.Count)
        {
            return new List<EventData>(allEvents);
        }

        List<EventData> availableEvents = new List<EventData>(allEvents);
        List<EventData> chosenEvents = new List<EventData>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableEvents.Count);
            chosenEvents.Add(availableEvents[randomIndex]);
            availableEvents.RemoveAt(randomIndex);
        }
        return chosenEvents;
    }

    public void SelectEvent(EventData selected)
    {
        selectedEvent = selected;
    }

    // ===== Proximity/Real location checking =====

    /// <summary>
    /// Check if player is near enough to check in to an event
    /// </summary>
    public bool IsEventNearby(string eventID)
    {
        EventData eventData = allEvents.Find(e => e.eventID == eventID);
        if (eventData == null)
        {
            Debug.LogWarning($"Event {eventID} not found");
            return false;
        }

        if (ProximityChecker.Instance == null)
        {
            Debug.LogWarning("ProximityChecker not found in scene");
            return false;
        }

        return ProximityChecker.Instance.IsPlayerNearEvent(eventData);
    }

    /// <summary>
    /// Get distance to an event location in meters
    /// </summary>
    public float GetDistanceToEvent(string eventID)
    {
        EventData eventData = allEvents.Find(e => e.eventID == eventID);
        if (eventData == null) return float.MaxValue;

        if (ProximityChecker.Instance == null) return float.MaxValue;

        return ProximityChecker.Instance.GetDistanceToEvent(eventData);
    }

    /// <summary>
    /// Check in to an event (only works if player is nearby)
    /// Awards points and marks as attended
    /// </summary>
    public bool CheckInToEvent(string eventID)
    {
        EventData eventData = allEvents.Find(e => e.eventID == eventID);
        
        if (eventData == null)
        {
            Debug.LogWarning($"Event {eventID} not found");
            return false;
        }

        // Check if already checked in
        if (eventData.hasAttended)
        {
            Debug.LogWarning($"Already checked in to '{eventData.eventTitle}'");
            return false;
        }

        // Check proximity
        if (!IsEventNearby(eventID))
        {
            float distance = GetDistanceToEvent(eventID);
            Debug.LogWarning($"Too far from event location. Distance: {distance:F0}m, Required: {eventData.activationRadius}m");
            return false;
        }

        // All checks passed - check in to event
        eventData.hasAttended = true;

        // Award points through UserProfileManager
        if (UserProfileManager.Instance != null)
        {
            UserProfileManager.Instance.AttendEvent(eventID, eventData.rewardPoints, (success, message) =>
            {
                if (success)
                {
                    Debug.Log($"✅ Checked in to event: {eventData.eventTitle} (+{eventData.rewardPoints} points)");
                }
                else
                {
                    Debug.LogError($"❌ Failed to award points: {message}");
                }
            });
        }
        else
        {
            Debug.LogError("UserProfileManager not found!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if player can check in to an event (is nearby and hasn't attended)
    /// </summary>
    public bool CanCheckInToEvent(string eventID)
    {
        EventData eventData = allEvents.Find(e => e.eventID == eventID);
        
        if (eventData == null) return false;
        if (eventData.hasAttended) return false;
        
        return IsEventNearby(eventID);
    }

}