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
}

// User profile data stored in Firebase Realtime Database
// IMPORTANT: NO PASSWORD stored here - Firebase Auth handles that securely
[System.Serializable]
public class UserProfile
{
    public string username;        // Unique username for the user
    public string displayName;     // User's display name (can contain spaces, special chars)
    public long createdTimestamp;  // When the account was created (Unix timestamp)

    // 🔒 SECURITY NOTE: Passwords are NEVER stored here!
    // Firebase Authentication handles password hashing and security

    // Creates a new user profile
    public UserProfile(string username, string displayName, long createdTimestamp)
    {
        this.username = username;
        this.displayName = displayName;
        // The timestamp is stored for time specific features (e.g. account age, verification, rewards, etc)
        this.createdTimestamp = createdTimestamp;
    }
}