using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;   // For Action<T>

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public DatabaseReference DbReference { get; private set; }

    // The line below is what connects the code to firebase
    private string databaseURL = "https://galactic-orbit-default-rtdb.firebaseio.com/";

    void Awake()
    {
        // Singleton pattern to ensure one instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseDatabase db = FirebaseDatabase.GetInstance(app, databaseURL);
                DbReference = db.RootReference;

                Debug.Log("✅ Firebase and Realtime Database initialized!");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
    }

    // Temp user creation method
    public void CreateUser(string userId, string username, string name, string password)
    {
        UserAccount user = new UserAccount(username, name, password);
        string json = JsonUtility.ToJson(user);
        DbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"User {userId} created successfully");
            }
            else
            {
                Debug.LogError("Failed to create user: " + task.Exception);
            }
        });
    }

    public void IsUsernameTaken(string usernameToCheck, Action<bool> callback)
    {
        DbReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var userSnapshot in task.Result.Children)
                {
                    var json = userSnapshot.GetRawJsonValue();
                    var user = JsonUtility.FromJson<UserAccount>(json);

                    if (user.username == usernameToCheck)
                    {
                        callback(true); // Username exists
                        return;
                    }
                }

                callback(false); // Username not found
            }
            else
            {
                Debug.LogError("Failed to check username: " + task.Exception);
                callback(false); // Assume not taken on error
            }
        });
    }

    // Read user data by userId, placed here for FirebaseTester, reorganize classes later :pray:
    public void GetUser(string userId, Action<UserAccount> onUserReceived)
    {
        DbReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error reading user: " + task.Exception);
                onUserReceived?.Invoke(null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    UserAccount user = JsonUtility.FromJson<UserAccount>(json);
                    onUserReceived?.Invoke(user);
                }
                else
                {
                    Debug.Log("User does not exist.");
                    onUserReceived?.Invoke(null);
                }
            }
        });
    }
}

[System.Serializable]
public class UserAccount
{
    public string username;
    public string name;
    public string password;

    public UserAccount(string username, string name, string password)
    {
        this.username = username;
        this.name = name;
        this.password = password;
    }

    
}
