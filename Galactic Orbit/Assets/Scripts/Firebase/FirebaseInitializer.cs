// MUST ATTACH TO GAME OBJECT IN FIRST SCENE

using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("✅ Firebase initialized");
            }
            else
            {
                Debug.LogError("❌ Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }
    // Ensure this object persists across scenes
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}

