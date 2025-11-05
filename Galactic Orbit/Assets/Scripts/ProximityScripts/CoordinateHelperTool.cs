using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper tool to automatically fill GPS coordinates for Events and Quests
/// based on their location names
/// </summary>
public class CoordinateHelperTool : MonoBehaviour
{
    [Header("Manual Lookup")]
    public string locationName = "Student Union";
    [Tooltip("Result will appear here")]
    public Vector2 foundCoordinates;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update manual lookup
        if (!string.IsNullOrEmpty(locationName))
        {
            foundCoordinates = UTDLocationDatabase.GetCoordinates(locationName);
        }
    }

    public void AutoFillAllEventCoordinates()
    {
        // Find EventManager in scene (works in Edit mode)
        EventManager eventManager = FindFirstObjectByType<EventManager>();
        
        if (eventManager == null)
        {
            Debug.LogError("EventManager not found in scene! Make sure there's a GameObject with EventManager component.");
            return;
        }

        if (eventManager.allEvents == null || eventManager.allEvents.Count == 0)
        {
            Debug.LogError("No events found in EventManager.allEvents!");
            return;
        }

        int updatedCount = 0;
        int skippedCount = 0;

        foreach (EventData eventData in eventManager.allEvents)
        {
            if (eventData == null) continue;

            // Try to get coordinates based on eventLocation (the building name)
            Vector2 coords = UTDLocationDatabase.GetCoordinates(eventData.eventLocation);

            // Always update - GetCoordinates will find the building even from room numbers
            eventData.gpsCoordinates = coords;
            EditorUtility.SetDirty(eventData);
            Debug.Log($"✅ Updated '{eventData.eventTitle}' at {eventData.eventLocation} → {coords}");
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Auto-fill complete: {updatedCount} updated, {skippedCount} skipped");
    }

    public void PrintAvailableLocations()
    {
        string[] locations = UTDLocationDatabase.GetAllLocationNames();
        Debug.Log("=== Available UTD Locations ===");
        foreach (string loc in locations)
        {
            Vector2 coords = UTDLocationDatabase.GetCoordinates(loc);
            Debug.Log($"{loc}: ({coords.x}, {coords.y})");
        }
        Debug.Log($"Total: {locations.Length} locations");
    }

    public void LookupLocation()
    {
        Vector2 coords = UTDLocationDatabase.GetCoordinates(locationName);
        Debug.Log($"'{locationName}' → ({coords.x}, {coords.y})");
        foundCoordinates = coords;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CoordinateHelperTool))]
public class CoordinateHelperToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CoordinateHelperTool tool = (CoordinateHelperTool)target;

        GUILayout.Space(10);
        
        if (GUILayout.Button("Auto-Fill All Event Coordinates", GUILayout.Height(30)))
        {
            tool.AutoFillAllEventCoordinates();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Print All Available Locations"))
        {
            tool.PrintAvailableLocations();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Lookup Location"))
        {
            tool.LookupLocation();
        }
    }
}
#endif