using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Database of real GPS coordinates for UTD campus locations
/// Use this to automatically assign coordinates to quests and events based on building names
/// </summary>
public static class UTDLocationDatabase
{
    // Dictionary mapping building names to GPS coordinates
    private static readonly Dictionary<string, Vector2> locations = new Dictionary<string, Vector2>()
    {
        // Main Academic Buildings
        { "Eugene McDermott Library", new Vector2(32.98693884293794f, -96.74760312082694f) },
        { "Student Union", new Vector2(32.9857f, -96.7501f) },
        { "Activity Center", new Vector2(32.9869f, -96.7489f) },
        { "Jonsson Academic Center", new Vector2(32.9863f, -96.7489f) },
        { "Engineering and Computer Science Complex", new Vector2(32.9881f, -96.7488f) },
        { "Natural Science and Engineering Research Laboratory", new Vector2(32.9890f, -96.7476f) },
        { "Callier Center", new Vector2(32.9851f, -96.7523f) },
        { "Founders Building", new Vector2(32.9849f, -96.7509f) },
        { "Green Center", new Vector2(32.9873f, -96.7508f) },
        { "Student Services Building", new Vector2(32.9855f, -96.7496f) },
        
        // Residence Halls
        { "Residence Hall West", new Vector2(32.9844f, -96.7475f) },
        { "Residence Hall North", new Vector2(32.9887f, -96.7503f) },
        { "Residence Hall South", new Vector2(32.9843f, -96.7496f) },
        
        // Other Notable Locations
        { "Parking Structure 1", new Vector2(32.9882f, -96.7514f) },
        { "Parking Structure 2", new Vector2(32.9847f, -96.7519f) },
        { "Parking Structure 3", new Vector2(32.9891f, -96.7462f) },
        { "Comet Statue", new Vector2(32.9858f, -96.7499f) },
        { "Chess Plaza", new Vector2(32.9871f, -96.7502f) },
        { "Reflection Pool", new Vector2(32.9862f, -96.7496f) },
        
        // Building Abbreviations & Aliases
        { "JSOM", new Vector2(32.9863f, -96.7489f) }, // Jonsson Academic Center
        { "SU", new Vector2(32.9857f, -96.7501f) }, // Student Union
        { "ECSS", new Vector2(32.9881f, -96.7488f) }, // Engineering and CS Complex
        { "ECS", new Vector2(32.9881f, -96.7488f) }, // Engineering and CS Complex
        { "SCI", new Vector2(32.9887f, -96.7508f) }, // Science Building (near Sirius Hall)
        { "NSERL", new Vector2(32.9890f, -96.7476f) }, // Natural Science and Engineering Research Laboratory
        { "SSB", new Vector2(32.9855f, -96.7496f) }, // Student Services Building
        { "SLC", new Vector2(32.9857f, -96.7501f) }, // Student Union (Student Leadership Center)
        
        // Specific Areas (use parent building coordinates)
        { "JSOM Atrium", new Vector2(32.9863f, -96.7489f) },
        { "SU Galaxy Rooms", new Vector2(32.9857f, -96.7501f) },
        { "SU Green Lawn", new Vector2(32.9857f, -96.7501f) },
        { "SCI Courtyard", new Vector2(32.9887f, -96.7508f) }, // Updated to correct SCI location
        
        // University Village (UV) - Off-campus housing
        { "University Village", new Vector2(32.9825f, -96.7525f) },
        { "UV", new Vector2(32.9825f, -96.7525f) },
        { "UV Housing Office", new Vector2(32.9825f, -96.7525f) },
        { "Phase 8 Clubhouse", new Vector2(32.9825f, -96.7525f) },
        { "UV Phase 8 Clubhouse", new Vector2(32.9825f, -96.7525f) },
        { "UV Clubhouse", new Vector2(32.9825f, -96.7525f) },
        { "UV Firepit", new Vector2(32.9825f, -96.7525f) },
        { "UV Fire Pit", new Vector2(32.9825f, -96.7525f) },
        { "University Village Firepit", new Vector2(32.9825f, -96.7525f) },
    };

    // UTD campus center (fallback if location not found)
    private static readonly Vector2 campusCenter = new Vector2(32.9857f, -96.7501f);

    /// <summary>
    /// Get GPS coordinates for a building name
    /// Returns campus center if building not found
    /// </summary>
    public static Vector2 GetCoordinates(string locationName)
    {
        if (string.IsNullOrEmpty(locationName))
        {
            Debug.LogWarning("Empty location name, using campus center");
            return campusCenter;
        }

        // Try exact match first
        if (locations.TryGetValue(locationName, out Vector2 coords))
        {
            return coords;
        }

        // Extract building code from room numbers (e.g., "ECSS 2.410" → "ECSS")
        string buildingCode = ExtractBuildingCode(locationName);
        if (!string.IsNullOrEmpty(buildingCode) && locations.TryGetValue(buildingCode, out Vector2 codeCoords))
        {
            Debug.Log($"Matched room '{locationName}' to building '{buildingCode}'");
            return codeCoords;
        }

        // Try partial match (case insensitive)
        foreach (var kvp in locations)
        {
            if (kvp.Key.ToLower().Contains(locationName.ToLower()) ||
                locationName.ToLower().Contains(kvp.Key.ToLower()))
            {
                Debug.Log($"Found partial match: '{locationName}' → '{kvp.Key}'");
                return kvp.Value;
            }
        }

        // Not found - use campus center
        Debug.LogWarning($"Location '{locationName}' not found in database, using campus center");
        return campusCenter;
    }

    /// <summary>
    /// Extract building code from room number (e.g., "ECSS 2.410" → "ECSS")
    /// </summary>
    private static string ExtractBuildingCode(string locationName)
    {
        if (string.IsNullOrEmpty(locationName))
            return null;

        // Split by space and take first part
        string[] parts = locationName.Split(' ');
        if (parts.Length > 1)
        {
            return parts[0]; // e.g., "ECSS" from "ECSS 2.410"
        }

        return null;
    }

    /// <summary>
    /// Check if a location exists in the database
    /// </summary>
    public static bool HasLocation(string locationName)
    {
        return locations.ContainsKey(locationName);
    }

    /// <summary>
    /// Get all available location names
    /// </summary>
    public static string[] GetAllLocationNames()
    {
        string[] names = new string[locations.Count];
        locations.Keys.CopyTo(names, 0);
        return names;
    }

    /// <summary>
    /// Get a random location from the database
    /// </summary>
    public static KeyValuePair<string, Vector2> GetRandomLocation()
    {
        string[] names = GetAllLocationNames();
        string randomName = names[Random.Range(0, names.Length)];
        return new KeyValuePair<string, Vector2>(randomName, locations[randomName]);
    }
}