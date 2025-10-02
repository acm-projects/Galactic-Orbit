using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using UnityEngine;

// Handles all Firebase Authentication operations
// Sign in, sign out, registration, password reset, and account deletion
public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

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

    // Registers a new user with Firebase Authentication and creates their profile
    public void RegisterUser(string email, string password, string username, string displayName, Action<bool, string> callback)
    {
        // Create user account with Firebase Authentication
        FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                // Registration successful - get the new user
                FirebaseUser newUser = task.Result.User;

                // Create user profile data (NO PASSWORD - Auth handles that securely)
                UserProfile profile = new UserProfile(username, displayName, email, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                string json = JsonUtility.ToJson(profile);

                // Save profile to database using the user's unique Firebase ID
                FirebaseManager.Instance.DbReference.Child("userProfiles").Child(newUser.UserId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(profileTask =>
                {
                    if (profileTask.IsCompleted)
                    {
                        callback?.Invoke(true, "Registration successful!");
                    }
                    else
                    {
                        callback?.Invoke(false, "Failed to create profile: " + profileTask.Exception?.Message);
                    }
                });
            }
            else
            {
                // Registration failed - return error message
                callback?.Invoke(false, "Registration failed: " + task.Exception?.Message);
            }
        });
    }

    // Signs in an existing user with email and password
    public void SignInUser(string email, string password, Action<bool, string> callback)
    {
        // Attempt to sign in with Firebase Authentication
        FirebaseManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                // Sign in successful
                callback?.Invoke(true, "Sign in successful!");
            }
            else
            {
                // Sign in failed - return error message
                callback?.Invoke(false, "Sign in failed: " + task.Exception?.Message);
            }
        });
    }

    // Signs out the current user
    public void SignOutUser()
    {
        // Firebase handles the sign out process
        FirebaseManager.Instance.Auth.SignOut();
        // OnAuthStateChanged will automatically be called and trigger OnUserSignedOut event
    }

    // Sends a password reset email to the given address
    public void SendPasswordResetEmail(string email, Action<bool, string> callback)
    {
        FirebaseManager.Instance.Auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
                callback?.Invoke(true, "Password reset email sent.");
            else
                callback?.Invoke(false, "Error: " + task.Exception?.Message);
        });
    }

    // Deletes the current user's account and profile data
    public void DeleteUser(Action<bool, string> callback)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
        {
            callback?.Invoke(false, "Not signed in");
            return;
        }

        string userId = currentUser.UserId;

        // First, remove the profile data from the database
        FirebaseManager.Instance.DbReference.Child("userProfiles").Child(userId).RemoveValueAsync().ContinueWithOnMainThread(dbTask =>
        {
            if (dbTask.IsCompleted)
            {
                // Then, delete the Firebase Auth account
                currentUser.DeleteAsync().ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCompleted)
                        callback?.Invoke(true, "User deleted successfully.");
                    else
                        callback?.Invoke(false, "Failed to delete account: " + authTask.Exception?.Message);
                });
            }
            else
            {
                callback?.Invoke(false, "Failed to delete profile: " + dbTask.Exception?.Message);
            }
        });
    }

    // Optional: Parses common Firebase exceptions into human-readable messages
    private string ParseFirebaseException(AggregateException exception)
    {
        if (exception == null) return "Unknown error.";

        foreach (var inner in exception.InnerExceptions)
        {
            if (inner is FirebaseException firebaseEx)
            {
                switch ((AuthError)firebaseEx.ErrorCode)
                {
                    case AuthError.InvalidEmail:
                        return "Invalid email address.";
                    case AuthError.EmailAlreadyInUse:
                        return "Email is already registered.";
                    case AuthError.WeakPassword:
                        return "Password is too weak.";
                    case AuthError.WrongPassword:
                        return "Incorrect password.";
                    case AuthError.UserNotFound:
                        return "User not found.";
                    case AuthError.NetworkRequestFailed:
                        return "Network error. Check your internet connection.";
                    default:
                        return $"Firebase error: {firebaseEx.Message}";
                }
            }
        }
        return exception.Message;
    }
}