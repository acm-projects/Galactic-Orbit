using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class UTDEventsSync : MonoBehaviour
{
    public static UTDEventsSync Instance;

    // UTD Comet Calendar API endpoint
    private const string UTD_EVENTS_API = "https://calendar.utdallas.edu/api/2/events";

    [Header("Event Storage")]
    public List<UTDEvent> currentEvents = new List<UTDEvent>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Wait a moment for EventManager to initialize, then sync
        Invoke(nameof(SyncEvents), 1f);
    }

    void SyncEvents()
    {
        SyncEventsToManager(30, 50);
    }

    // Fetch and sync UTD events to EventManager
    public void SyncEventsToManager(int days = 30, int count = 50)
    {
        FetchUpcomingEvents(days, count, (utdEvents) => {
            if (EventManager.Instance == null)
            {
                Debug.LogError("EventManager not found!");
                return;
            }

            Debug.Log($"🔄 Syncing {utdEvents.Count} UTD events to EventManager...");

            // Convert UTDEvents to your EventData class and add to EventManager
            foreach (var utdEvent in utdEvents)
            {
                EventData gameEvent = ConvertToEvent(utdEvent);
                EventManager.Instance.allEvents.Add(gameEvent);
            }

            Debug.Log($"✅ EventManager now has {EventManager.Instance.allEvents.Count} total events");
        });
    }

    // Fetch upcoming UTD events
    private void FetchUpcomingEvents(int days, int count, Action<List<UTDEvent>> onComplete)
    {
        StartCoroutine(FetchEventsCoroutine(days, count, onComplete));
    }

    private IEnumerator FetchEventsCoroutine(int days, int count, Action<List<UTDEvent>> onComplete)
    {
        // Build API URL with parameters
        string url = $"{UTD_EVENTS_API}?days={days}&pp={count}";

        Debug.Log($"📡 Fetching UTD events from: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"✅ Received UTD events data");

                // Parse the JSON response
                List<UTDEvent> events = ParseEventsFromJSON(jsonResponse);
                Debug.Log($"📅 Parsed {events.Count} events");

                // Store events
                currentEvents = events;

                onComplete?.Invoke(events);
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch UTD events: {request.error}");
                onComplete?.Invoke(new List<UTDEvent>());
            }
        }
    }

    // Parse JSON response into UTDEvent objects
    private List<UTDEvent> ParseEventsFromJSON(string json)
    {
        List<UTDEvent> events = new List<UTDEvent>();

        try
        {
            var N = JSON.Parse(json);
            
            if (N == null || N["events"] == null)
            {
                Debug.LogWarning("No events array found in response");
                return events;
            }

            var eventsArray = N["events"].AsArray;
            
            // FIX: Use for loop instead of foreach
            for (int i = 0; i < eventsArray.Count; i++)
            {
                var eventNode = eventsArray[i];
                
                if (eventNode == null) continue;
                
                // Check if event has instances
                if (eventNode["event_instances"] == null || eventNode["event_instances"].Count == 0)
                    continue;

                var firstInstance = eventNode["event_instances"][0]["event_instance"];
                
                UTDEvent utdEvent = new UTDEvent
                {
                    id = eventNode["id"].ToString(),
                    title = eventNode["title"] ?? "Untitled Event",
                    description = CleanDescription(eventNode["description"]),
                    location = GetLocationFromNode(eventNode),
                    latitude = GetLatitudeFromNode(eventNode),
                    longitude = GetLongitudeFromNode(eventNode),
                    startTime = firstInstance["start"] ?? "",
                    endTime = firstInstance["end"] ?? "",
                    url = eventNode["localist_url"] ?? ""
                };

                events.Add(utdEvent);
                Debug.Log($"  📌 Parsed event: {utdEvent.title}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing events: {e.Message}\n{e.StackTrace}");
        }

        return events;
    }

    // Helper methods for SimpleJSON
    private string GetLocationFromNode(JSONNode eventNode)
    {
        if (eventNode["location"] != null && eventNode["location"]["name"] != null)
            return eventNode["location"]["name"];
        
        if (eventNode["location_name"] != null)
            return eventNode["location_name"];
        
        return "UTD Campus";
    }

    private float GetLatitudeFromNode(JSONNode eventNode)
    {
        if (eventNode["location"] != null && eventNode["location"]["latitude"] != null)
            return eventNode["location"]["latitude"].AsFloat;
        
        return 32.9857f; // Default: UTD Student Union
    }

    private float GetLongitudeFromNode(JSONNode eventNode)
    {
        if (eventNode["location"] != null && eventNode["location"]["longitude"] != null)
            return eventNode["location"]["longitude"].AsFloat;
        
        return -96.7501f; // Default: UTD Student Union
    }

    // Clean HTML from description
    private string CleanDescription(string html)
    {
        if (string.IsNullOrEmpty(html)) return "No description available.";

        // Basic HTML tag removal
        string text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        text = System.Text.RegularExpressions.Regex.Replace(text, "&nbsp;", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, "&amp;", "&");
        text = System.Text.RegularExpressions.Regex.Replace(text, "&quot;", "\"");
        text = text.Trim();

        return text;
    }

    // Convert UTDEvent to your EventData class
    private EventData ConvertToEvent(UTDEvent utdEvent)
    {
        EventData gameEvent = ScriptableObject.CreateInstance<EventData>();
        
        gameEvent.eventTitle = utdEvent.title;
        gameEvent.eventLocation = utdEvent.location;
        
        // Parse and format the date/time
        try
        {
            System.DateTime dateTime = System.DateTime.Parse(utdEvent.startTime);
            gameEvent.eventDay = dateTime.ToString("dddd, MMMM dd"); // e.g., "Monday, October 15"
            gameEvent.eventTime = dateTime.ToString("h:mm tt"); // e.g., "2:30 PM"
        }
        catch
        {
            gameEvent.eventDay = "TBA";
            gameEvent.eventTime = "TBA";
        }
        
        return gameEvent;
    }
}

// ===== EVENT DATA CLASS =====

[Serializable]
public class UTDEvent
{
    public string id;
    public string title;
    public string description;
    public string location;
    public float latitude;
    public float longitude;
    public string startTime;
    public string endTime;
    public string url;
}