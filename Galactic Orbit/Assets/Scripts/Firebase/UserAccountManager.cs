// MUST ATTACH TO GAME OBJECT IN FIRST SCENE
// Replace this with Firebase Authentication later, this is just for now

using UnityEngine;
using Firebase;
using Firebase.Database;    // Do I need this specifically?
using Firebase.Extensions;  // Do I need this specifically?
using System;



public class UserAccountManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    void Awake()
    {
        string databaseURL = "https://galactic-orbit-default-rtdb.firebaseio.com/";
        FirebaseApp app = FirebaseApp.DefaultInstance;
        FirebaseDatabase db = FirebaseDatabase.GetInstance(app, databaseURL);
        dbReference = db.RootReference;
    }


    // Create a new user account
    public void CreateUser(string userId, string username, string name, string password)
    {
        User newUser = new User(username, name, password);
        string json = JsonUtility.ToJson(newUser);

        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"✅ User {username} created successfully.");
            }
            else
            {
                Debug.LogError($"❌ Failed to create user: {task.Exception}");
            }
        });
    }

    // Read user data by userId
    public void GetUser(string userId, Action<User> onUserReceived)
    {
        dbReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    User user = JsonUtility.FromJson<User>(snapshot.GetRawJsonValue());
                    onUserReceived?.Invoke(user);
                }
                else
                {
                    Debug.LogWarning($"User with ID {userId} does not exist.");
                    onUserReceived?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to read user data: {task.Exception}");
                onUserReceived?.Invoke(null);
            }
        });
    }
}

[Serializable]
public class User
{
    public string username;
    public string name;
    public string password;

    public User(string username, string name, string password)
    {
        this.username = username;
        this.name = name;
        this.password = password;
    }
}
