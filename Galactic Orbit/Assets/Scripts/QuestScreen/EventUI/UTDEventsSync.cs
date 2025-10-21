using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

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
            // Parse the wrapper
            LocalistResponse response = JsonUtility.FromJson<LocalistResponse>(json);

            if (response != null && response.events != null)
            {
                foreach (var eventData in response.events)
                {
                    if (eventData.event_instances != null && eventData.event_instances.Length > 0)
                    {
                        // Use the first instance
                        var instance = eventData.event_instances[0];

                        UTDEvent utdEvent = new UTDEvent
                        {
                            id = eventData.id.ToString(),
                            title = eventData.title,
                            description = CleanDescription(eventData.description),
                            location = GetLocationString(eventData),
                            latitude = GetLatitude(eventData),
                            longitude = GetLongitude(eventData),
                            startTime = instance.event_instance.start,
                            endTime = instance.event_instance.end,
                            url = eventData.localist_url
                        };

                        events.Add(utdEvent);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing events: {e.Message}");
        }

        return events;
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

    // Extract location string
    private string GetLocationString(LocalistEventData eventData)
    {
        if (eventData.location != null && !string.IsNullOrEmpty(eventData.location.name))
        {
            return eventData.location.name;
        }
        if (!string.IsNullOrEmpty(eventData.location_name))
        {
            return eventData.location_name;
        }
        return "UTD Campus";
    }

    // Get latitude
    private float GetLatitude(LocalistEventData eventData)
    {
        if (eventData.location != null && eventData.location.latitude != 0)
        {
            return eventData.location.latitude;
        }
        return 32.9857f; // Default: UTD Student Union
    }

    // Get longitude
    private float GetLongitude(LocalistEventData eventData)
    {
        if (eventData.location != null && eventData.location.longitude != 0)
        {
            return eventData.location.longitude;
        }
        return -96.7501f; // Default: UTD Student Union
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

// ===== JSON PARSING CLASSES =====

[Serializable]
public class LocalistResponse
{
    public LocalistEventData[] events;
}

[Serializable]
public class LocalistEventData
{
    public int id;
    public string title;
    public string description;
    public string location_name;
    public string localist_url;
    public LocalistLocation location;
    public LocalistEventInstanceWrapper[] event_instances;
}

[Serializable]
public class LocalistLocation
{
    public string name;
    public float latitude;
    public float longitude;
}

[Serializable]
public class LocalistEventInstanceWrapper
{
    public LocalistEventInstance event_instance;
}

[Serializable]
public class LocalistEventInstance
{
    public string start;
    public string end;
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