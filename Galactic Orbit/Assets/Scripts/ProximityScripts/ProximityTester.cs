using UnityEngine;

/// <summary>
/// Test script for proximity-based quest/event activation
/// Press keys to test backend functionality before hooking up UI
/// </summary>
public class ProximityTester : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode testQuestProximityKey = KeyCode.Q;
    public KeyCode testEventProximityKey = KeyCode.E;
    public KeyCode testStartQuestKey = KeyCode.S;
    public KeyCode testCheckInEventKey = KeyCode.C;

    [Header("Test Data")]
    public string testQuestID = ""; // Fill in a quest ID from your game
    public string testEventID = ""; // Fill in an event ID from your game

    void Update()
    {
        // Test quest proximity
        if (Input.GetKeyDown(testQuestProximityKey))
        {
            TestQuestProximity();
        }

        // Test event proximity
        if (Input.GetKeyDown(testEventProximityKey))
        {
            TestEventProximity();
        }

        // Test starting quest
        if (Input.GetKeyDown(testStartQuestKey))
        {
            TestStartQuest();
        }

        // Test checking in to event
        if (Input.GetKeyDown(testCheckInEventKey))
        {
            TestCheckInEvent();
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        string gpsStatus = "GPS: Not Available";
        if (GPSManager.Instance != null && GPSManager.Instance.HasGPS)
        {
            gpsStatus = $"GPS: ({GPSManager.Instance.latitude:F4}, {GPSManager.Instance.longitude:F4})";
        }

        GUI.Label(new Rect(10, 10, 600, 400),
            "=== Proximity Backend Tester ===\n\n" +
            $"{gpsStatus}\n\n" +
            $"[{testQuestProximityKey}] Check Quest Proximity\n" +
            $"[{testEventProximityKey}] Check Event Proximity\n" +
            $"[{testStartQuestKey}] Try Start Quest\n" +
            $"[{testCheckInEventKey}] Try Check In Event\n\n" +
            "Set testQuestID and testEventID in Inspector!\n" +
            "Check Console for results!",
            style);
    }

    void TestQuestProximity()
    {
        if (string.IsNullOrEmpty(testQuestID))
        {
            Debug.LogError("❌ Set testQuestID in Inspector first!");
            return;
        }

        Debug.Log("=== TEST: Quest Proximity ===");

        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ QuestManager not found!");
            return;
        }

        bool isNearby = QuestManager.Instance.IsQuestNearby(testQuestID);
        float distance = QuestManager.Instance.GetDistanceToQuest(testQuestID);

        Debug.Log($"Quest ID: {testQuestID}");
        Debug.Log($"Is Nearby: {isNearby}");
        Debug.Log($"Distance: {distance:F1}m");

        if (isNearby)
        {
            Debug.Log("✅ Player is close enough to start this quest!");
        }
        else
        {
            Debug.Log($"⚠️ Player is too far. Get within range to start.");
        }
    }

    void TestEventProximity()
    {
        if (string.IsNullOrEmpty(testEventID))
        {
            Debug.LogError("❌ Set testEventID in Inspector first!");
            return;
        }

        Debug.Log("=== TEST: Event Proximity ===");

        if (EventManager.Instance == null)
        {
            Debug.LogError("❌ EventManager not found!");
            return;
        }

        bool isNearby = EventManager.Instance.IsEventNearby(testEventID);
        float distance = EventManager.Instance.GetDistanceToEvent(testEventID);

        Debug.Log($"Event ID: {testEventID}");
        Debug.Log($"Is Nearby: {isNearby}");
        Debug.Log($"Distance: {distance:F1}m");

        if (isNearby)
        {
            Debug.Log("✅ Player is close enough to check in!");
        }
        else
        {
            Debug.Log($"⚠️ Player is too far. Get within range to check in.");
        }
    }

    void TestStartQuest()
    {
        if (string.IsNullOrEmpty(testQuestID))
        {
            Debug.LogError("❌ Set testQuestID in Inspector first!");
            return;
        }

        Debug.Log("=== TEST: Start Quest ===");

        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ QuestManager not found!");
            return;
        }

        bool canStart = QuestManager.Instance.CanStartQuest(testQuestID);
        Debug.Log($"Can Start Quest: {canStart}");

        if (canStart)
        {
            bool started = QuestManager.Instance.StartQuest(testQuestID);
            if (started)
            {
                Debug.Log("✅ Quest started successfully!");
            }
            else
            {
                Debug.LogError("❌ Failed to start quest");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot start quest (too far, already active, or completed)");
        }
    }

    void TestCheckInEvent()
    {
        if (string.IsNullOrEmpty(testEventID))
        {
            Debug.LogError("❌ Set testEventID in Inspector first!");
            return;
        }

        Debug.Log("=== TEST: Check In Event ===");

        if (EventManager.Instance == null)
        {
            Debug.LogError("❌ EventManager not found!");
            return;
        }

        bool canCheckIn = EventManager.Instance.CanCheckInToEvent(testEventID);
        Debug.Log($"Can Check In: {canCheckIn}");

        if (canCheckIn)
        {
            bool checkedIn = EventManager.Instance.CheckInToEvent(testEventID);
            if (checkedIn)
            {
                Debug.Log("✅ Checked in successfully! Points awarded.");
            }
            else
            {
                Debug.LogError("❌ Failed to check in");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot check in (too far or already attended)");
        }
    }
}