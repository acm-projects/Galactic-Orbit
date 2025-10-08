using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseDatabaseManager : MonoBehaviour
{
    string userId;
    DatabaseReference reference;

    void Start()
    {
        userId = SystemInfo.deviceUniqueIdentifier;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        CreateNewUser();
    }

    public void CreateNewUser()
    {
        reference.Child("users").Child(userId).SetValueAsync("John Doe");
        Debug.Log("New User Created");
    }
}
