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

        GetCurrentUserProfile((profile) =>
        {
            if (profile != null)
            {
                int newTotal = profile.coins + amount;
                
                var updates = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "coins", newTotal }
                };

                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                            callback?.Invoke(true, $"Added {amount} coins! Total: {newTotal}");
                        else
                            callback?.Invoke(false, "Failed to add coins: " + task.Exception?.Message);
                    });
            }
            else
            {
                callback?.Invoke(false, "Profile not found");
            }
        });
    }

    /// <summary>
    /// Add items to the current user
    /// </summary>
    public void AddItems(int amount, Action<bool, string> callback)
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
                int newTotal = profile.items + amount;
                
                var updates = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "items", newTotal }
                };

                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                            callback?.Invoke(true, $"Added {amount} items! Total: {newTotal}");
                        else
                            callback?.Invoke(false, "Failed to add items: " + task.Exception?.Message);
                    });
            }
            else
            {
                callback?.Invoke(false, "Profile not found");
            }
        });
    }

    /// <summary>
    /// Spend coins (deduct from balance)
    /// </summary>
    public void SpendCoins(int amount, Action<bool, string> callback)
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
                // Check if user has enough coins
                if (profile.coins < amount)
                {
                    callback?.Invoke(false, $"Not enough coins! Have {profile.coins}, need {amount}");
                    return;
                }

                int newTotal = profile.coins - amount;
                
                var updates = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "coins", newTotal }
                };

                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(currentUser.UserId)
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                            callback?.Invoke(true, $"Spent {amount} coins! Remaining: {newTotal}");
                        else
                            callback?.Invoke(false, "Failed to spend coins: " + task.Exception?.Message);
                    });
            }
            else
            {
                callback?.Invoke(false, "Profile not found");
            }
        });
    }
}

// ===== USER PROFILE DATA CLASS =====

[System.Serializable]
public class UserProfile
{
    // === REQUIRED AT SIGNUP ===
    public string username;
    public string email;
    public long createdTimestamp;

    // === OPTIONAL - CAN BE EDITED LATER ===
    public string displayName;
    public string bio;
    public string avatarUrl;

    // === GAME PROGRESS - STARTS AT ZERO ===
    public int totalPoints;
    public int level;
    public int eventsAttended;
    public int questsCompleted;
    public int buildingsVisited;

    // === CURRENCY & ITEMS ===
    public int coins;
    public int items;

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

        this.displayName = username;
        this.bio = "";
        this.avatarUrl = "";

        this.totalPoints = 0;
        this.level = 1;
        this.eventsAttended = 0;
        this.questsCompleted = 0;
        this.buildingsVisited = 0;

        this.coins = 0;
        this.items = 0;

        this.completedQuestIds = new string[0];
        this.visitedBuildingIds = new string[0];
        this.attendedEventIds = new string[0];
        this.teamIds = new string[0];
        this.scannedObjectIds = new string[0];
    }
}