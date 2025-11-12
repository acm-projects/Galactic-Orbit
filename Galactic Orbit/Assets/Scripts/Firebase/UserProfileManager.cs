using Firebase.Auth;
using Firebase.Extensions;
using System;
using UnityEngine;

// Manages user profile data in Firebase Realtime Database
// Handles reading, updating, and querying user profiles
public class UserProfileManager : MonoBehaviour
{
    public static UserProfileManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern: destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    // Get the profile data for the currently signed-in user
    public void GetCurrentUserProfile(Action<UserProfile> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;

        // Check if anyone is signed in
        if (currentUser == null)
        {
            callback?.Invoke(null);
            return;
        }

        // Fetch profile from database using the user's Firebase ID
        FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                // Profile found - convert JSON to UserProfile object
                string json = task.Result.GetRawJsonValue();
                UserProfile profile = JsonUtility.FromJson<UserProfile>(json);
                callback?.Invoke(profile);
            }
            else
            {
                // Profile not found or error occurred
                callback?.Invoke(null);
            }
        });
    }

    // Updates the display name of the current user
    public void UpdateUserProfile(string newDisplayName, Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;

        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
            .Child("displayName").SetValueAsync(newDisplayName)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    callback?.Invoke(true, "Profile updated");
                else
                    callback?.Invoke(false, "Update failed: " + task.Exception?.Message);
            });
    }

    // Checks if a username is already taken by another user
    public void IsUsernameTaken(string username, Action<bool> callback)
    {
        // Query database for profiles with matching username
        // OrderByChild + EqualTo creates an efficient database query
        FirebaseManager.Instance.DbReference.Child("userProfiles").OrderByChild("username").EqualTo(username).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // If any results found, username is taken
                callback?.Invoke(task.Result.ChildrenCount > 0);
            }
            else
            {
                // Error occurred - assume username is available to be safe
                Debug.LogError("Failed to check username: " + task.Exception);
                callback?.Invoke(false);
            }
        });
    }

    // Award points to the current user
    public void AddPoints(int points, Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;

        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        // Get current points first
        GetCurrentUserProfile((profile) =>
        {
            if (profile != null)
            {
                int newTotal = profile.totalPoints + points;
                int newLevel = CalculateLevel(newTotal);

                // Update both points and level
                var updates = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "totalPoints", newTotal },
                    { "level", newLevel }
                };

                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                            callback?.Invoke(true, $"Added {points} points! Total: {newTotal}, Level: {newLevel}");
                        else
                            callback?.Invoke(false, "Failed to add points: " + task.Exception?.Message);
                    });
            }
            else
            {
                callback?.Invoke(false, "Profile not found");
            }
        });
    }

    // Calculate level based on total points (customize this formula as needed)
    private int CalculateLevel(int totalPoints)
    {
        // Simple formula: 1 level per 100 points
        // Customize this based on your game design
        return 1 + (totalPoints / 100);
    }

    // Update a single field in the profile
    public void UpdateProfileField(string fieldName, object value, Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;

        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
            .Child(fieldName).SetValueAsync(value)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    callback?.Invoke(true, $"{fieldName} updated");
                else
                    callback?.Invoke(false, "Update failed: " + task.Exception?.Message);
            });
    }

    // Mark an event as attended
    public void AttendEvent(string eventId, int pointsEarned, Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;

        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        GetCurrentUserProfile((profile) =>
        {
            if (profile != null)
            {
                // Add event ID to attended list
                var eventsList = new System.Collections.Generic.List<string>(profile.attendedEventIds);
                if (!eventsList.Contains(eventId))
                {
                    eventsList.Add(eventId);
                }

                var updates = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "attendedEventIds", eventsList.ToArray() },
                    { "eventsAttended", eventsList.Count }
                };

                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            // Award points after marking attendance
                            AddPoints(pointsEarned, callback);
                        }
                        else
                        {
                            callback?.Invoke(false, "Failed to record attendance: " + task.Exception?.Message);
                        }
                    });
            }
        });
    }

    // Save complete character customization
    public void SaveCharacterCustomization(CharacterCustomization customization, Action<bool, string> callback)
    // ===== CURRENCY METHODS =====

    /// <summary>
    /// Add coins to the current user
    /// </summary>
    public void AddCoins(int amount, Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "primaryColorR", customization.primaryColor.r },
            { "primaryColorG", customization.primaryColor.g },
            { "primaryColorB", customization.primaryColor.b },
            
            { "secondaryColorR", customization.secondaryColor.r },
            { "secondaryColorG", customization.secondaryColor.g },
            { "secondaryColorB", customization.secondaryColor.b },
            
            { "tertiaryColorR", customization.tertiaryColor.r },
            { "tertiaryColorG", customization.tertiaryColor.g },
            { "tertiaryColorB", customization.tertiaryColor.b },
            
            { "accent1ColorR", customization.accent1Color.r },
            { "accent1ColorG", customization.accent1Color.g },
            { "accent1ColorB", customization.accent1Color.b },
            
            { "accent2ColorR", customization.accent2Color.r },
            { "accent2ColorG", customization.accent2Color.g },
            { "accent2ColorB", customization.accent2Color.b },
            
            { "skinColorR", customization.skinColor.r },
            { "skinColorG", customization.skinColor.g },
            { "skinColorB", customization.skinColor.b },
            
            { "selectedEyes", customization.selectedEyes },
            { "selectedMouth", customization.selectedMouth },
            { "selectedFaceDecoration", customization.selectedFaceDecoration }
        };

        FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    callback?.Invoke(true, "Character customization saved");
                else
                    callback?.Invoke(false, "Failed to save customization: " + task.Exception?.Message);
            });
    }

    // Load character customization
    public void LoadCharacterCustomization(Action<CharacterCustomization> callback)
    {
        GetCurrentUserProfile((profile) =>
        {
            if (profile != null)
            {
                callback?.Invoke(CharacterCustomization.FromProfile(profile));
            }
            else
            {
                callback?.Invoke(new CharacterCustomization());
            }
        });
    }
}

// User profile data stored in Firebase Realtime Database
// IMPORTANT: NO PASSWORD stored here - Firebase Auth handles that securely
[System.Serializable]
public class UserProfile
{
    // === REQUIRED AT SIGNUP ===
    public string username;           // Unique username
    public string email;              // Email address
    public long createdTimestamp;     // Account creation time

    // === OPTIONAL - CAN BE EDITED LATER ===
    public string displayName;        // Display name (starts as username)
    public string bio;                // User bio/description
    public string avatarUrl;          // Profile picture URL

    // === CHARACTER CUSTOMIZATION ===
    public float primaryColorR, primaryColorG, primaryColorB;
    public float secondaryColorR, secondaryColorG, secondaryColorB;
    public float tertiaryColorR, tertiaryColorG, tertiaryColorB;
    public float accent1ColorR, accent1ColorG, accent1ColorB;
    public float accent2ColorR, accent2ColorG, accent2ColorB;
    public float skinColorR, skinColorG, skinColorB;

    // Face customization
    public string selectedEyes;
    public string selectedMouth;
    public string selectedFaceDecoration;

    // === GAME PROGRESS - STARTS AT ZERO ===
    public int totalPoints;           // Total points earned
    public int level;                 // Player level (calculated from points)
    public int eventsAttended;        // Number of events attended
    public int questsCompleted;       // Number of quests completed
    public int buildingsVisited;      // Number of campus buildings visited

    // === LISTS - TRACK DETAILED PROGRESS ===
    public string[] completedQuestIds;
    public string[] visitedBuildingIds;
    public string[] attendedEventIds;
    public string[] teamIds;
    public string[] scannedObjectIds;

    // Constructor - only requires signup data
    public UserProfile(string username, string email, long createdTimestamp)
    {
        this.username = username;
        this.email = email;
        this.createdTimestamp = createdTimestamp;

        // Optional fields - start with defaults
        this.displayName = username;  // Start as username
        this.bio = "";
        this.avatarUrl = "";

        // Default colors (white)
        primaryColorR = primaryColorG = primaryColorB = 1f;
        secondaryColorR = secondaryColorG = secondaryColorB = 1f;
        tertiaryColorR = tertiaryColorG = tertiaryColorB = 1f;
        accent1ColorR = accent1ColorG = accent1ColorB = 1f;
        accent2ColorR = accent2ColorG = accent2ColorB = 1f;
        skinColorR = skinColorG = skinColorB = 1f;

        // Default face options
        selectedEyes = "Eyes1";
        selectedMouth = "Mouth1";
        selectedFaceDecoration = "Decor1";

        // Game progress - start at zero
        this.totalPoints = 0;
        this.level = 1;
        this.eventsAttended = 0;
        this.questsCompleted = 0;
        this.buildingsVisited = 0;

        // Lists - start empty
        this.coins = 0;
        this.items = 0;

        this.completedQuestIds = new string[0];
        this.visitedBuildingIds = new string[0];
        this.attendedEventIds = new string[0];
        this.teamIds = new string[0];
        this.scannedObjectIds = new string[0];
    }
}

[System.Serializable]
public class CharacterCustomization
{
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.white;
    public Color tertiaryColor = Color.white;
    public Color accent1Color = Color.white;
    public Color accent2Color = Color.white;
    public Color skinColor = Color.white;
    
    public string selectedEyes = "Eyes1";
    public string selectedMouth = "Mouth1";
    public string selectedFaceDecoration = "Decor1";

    // Convert to UserProfile format
    public void ApplyToProfile(UserProfile profile)
    {
        profile.primaryColorR = primaryColor.r;
        profile.primaryColorG = primaryColor.g;
        profile.primaryColorB = primaryColor.b;
        
        profile.secondaryColorR = secondaryColor.r;
        profile.secondaryColorG = secondaryColor.g;
        profile.secondaryColorB = secondaryColor.b;
        
        profile.tertiaryColorR = tertiaryColor.r;
        profile.tertiaryColorG = tertiaryColor.g;
        profile.tertiaryColorB = tertiaryColor.b;
        
        profile.accent1ColorR = accent1Color.r;
        profile.accent1ColorG = accent1Color.g;
        profile.accent1ColorB = accent1Color.b;
        
        profile.accent2ColorR = accent2Color.r;
        profile.accent2ColorG = accent2Color.g;
        profile.accent2ColorB = accent2Color.b;
        
        profile.skinColorR = skinColor.r;
        profile.skinColorG = skinColor.g;
        profile.skinColorB = skinColor.b;
        
        profile.selectedEyes = selectedEyes;
        profile.selectedMouth = selectedMouth;
        profile.selectedFaceDecoration = selectedFaceDecoration;
    }

    // Load from UserProfile
    public static CharacterCustomization FromProfile(UserProfile profile)
    {
        return new CharacterCustomization
        {
            primaryColor = new Color(profile.primaryColorR, profile.primaryColorG, profile.primaryColorB),
            secondaryColor = new Color(profile.secondaryColorR, profile.secondaryColorG, profile.secondaryColorB),
            tertiaryColor = new Color(profile.tertiaryColorR, profile.tertiaryColorG, profile.tertiaryColorB),
            accent1Color = new Color(profile.accent1ColorR, profile.accent1ColorG, profile.accent1ColorB),
            accent2Color = new Color(profile.accent2ColorR, profile.accent2ColorG, profile.accent2ColorB),
            skinColor = new Color(profile.skinColorR, profile.skinColorG, profile.skinColorB),
            selectedEyes = profile.selectedEyes ?? "Eyes1",
            selectedMouth = profile.selectedMouth ?? "Mouth1",
            selectedFaceDecoration = profile.selectedFaceDecoration ?? "Decor1"
        };
    }
}