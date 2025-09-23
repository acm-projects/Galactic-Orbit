using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;

public class FirebaseTester : MonoBehaviour
{
    private void Start()
    {
        // Wait a second to make sure FirebaseManager is initialized
        Invoke(nameof(RunTests), 1.0f);
    }

    void RunTests()
    {
        string testUserId = "testuser123";
        string testUsername = "tester99";
        string testName = "Test User";
        string testPassword = "testpass";

        // 1. Check if username is taken
        FirebaseManager.Instance.IsUsernameTaken(testUsername, (isTaken) =>
        {
            if (isTaken)
            {
                Debug.Log("❌ Username already taken. Choose another.");
            }
            else
            {
                Debug.Log("✅ Username available. Creating user...");

                // 2. Create user
                FirebaseManager.Instance.CreateUser(testUserId, testUsername, testName, testPassword);

                // 3. Read user after 2 seconds
                Invoke(nameof(ReadTestUser), 2f);
            }
        });
    }

    void ReadTestUser()
    {
        string testUserId = "testuser123";

        FirebaseManager.Instance.GetUser(testUserId, (user) =>
        {
            if (user != null)
            {
                Debug.Log("🎉 User loaded:");
                Debug.Log($"Username: {user.username}");
                Debug.Log($"Name: {user.name}");
                Debug.Log($"Password: {user.password}");
            }
            else
            {
                Debug.Log("⚠️ User not found.");
            }
        });
    }
}
