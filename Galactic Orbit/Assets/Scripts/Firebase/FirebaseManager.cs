using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

// Core Firebase initialization and infrastructure
// Handles Firebase app setup, authentication service, and database connection
// ATTACH TO A GAMEOBJECT IN FIRST SCENE
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    // === FIREBASE REFERENCES ===
    // Reference to Firebase Realtime Database root
    public DatabaseReference DbReference { get; private set; }
    // Reference to Firebase Authentication service
    public FirebaseAuth Auth { get; private set; }
    // Quick access to currently signed-in user (null if not signed in)
    public FirebaseUser CurrentUser => Auth?.CurrentUser;

    // === EVENTS ===
    // Other scripts can subscribe to these to react to auth changes
    public event Action<FirebaseUser> OnUserSignedIn;  // Called when user signs in
    public event Action OnUserSignedOut;               // Called when user signs out

    // === CONFIGURATION ===
    // Firebase project's database URL
    private string databaseURL = "https://galactic-orbit-default-rtdb.firebaseio.com/";

    // Unity Awake - Sets up singleton pattern and initializes Firebase
    void Awake()
    {
        Debug.Log("Waking Up");
        // Singleton pattern: destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);

        Debug.Log("Initializing Firebase");
        // Start Firebase initialization
        InitializeFirebase();
    }

    // Initializes Firebase services (Auth and Database)
    // Must be called after Firebase dependencies are resolved
    void InitializeFirebase()
    {
        // Check if all Firebase dependencies are available
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Get the default Firebase app instance
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // === INITIALIZE AUTHENTICATION ===
                Auth = FirebaseAuth.DefaultInstance;
                // Listen for authentication state changes (sign in/out)
                Auth.StateChanged += OnAuthStateChanged;

                // === INITIALIZE REALTIME DATABASE ===
                FirebaseDatabase db = FirebaseDatabase.GetInstance(app, databaseURL);
                DbReference = db.RootReference;

                Debug.Log("✅ Firebase Auth and Database initialized!");
            }
            else
            {
                Debug.LogError($"Firebase dependency error: {task.Result}");
            }
        });
    }

    // Called automatically whenever authentication state changes
    // Triggers our custom events that other scripts can listen to
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        // Check if someone is currently signed in
        if (CurrentUser != null)
        {
            // User signed in - notify all subscribers
            OnUserSignedIn?.Invoke(CurrentUser);
        }
        else
        {
            // User signed out - notify all subscribers
            OnUserSignedOut?.Invoke();
        }
    }

    // Signs in a user with either email or username
    // Automatically detects if input is email (contains @) or username
    // Returns true if login successful, false otherwise
    public async Task<bool> LoginAsync(string emailOrUsername, string password)
    {
        try
        {
            string email = emailOrUsername;
            
            Debug.Log($"Login attempt with: {emailOrUsername}");
            
            // Check if input looks like an email (contains @)
            if (!emailOrUsername.Contains("@"))
            {
                Debug.Log("Input detected as USERNAME. Looking up email...");
                
                // It's a username - look up the email from user profile
                var snapshot = await DbReference.Child("userProfiles")
                    .OrderByChild("username")
                    .EqualTo(emailOrUsername)
                    .GetValueAsync();
                
                Debug.Log($"Snapshot exists: {snapshot.Exists}, Children count: {snapshot.ChildrenCount}");
                
                if (snapshot.Exists && snapshot.ChildrenCount > 0)
                {
                    // Get the email from the profile
                    foreach (var child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        Debug.Log($"Retrieved JSON: {json}");
                        
                        UserProfile profile = JsonUtility.FromJson<UserProfile>(json);
                        email = profile.email;
                        
                        Debug.Log($"Found email for username '{emailOrUsername}': {email}");
                        break;
                    }
                }
                else
                {
                    Debug.LogError($"Username '{emailOrUsername}' not found in database");
                    return false;
                }
            }
            else
            {
                Debug.Log("Input detected as EMAIL. Using directly.");
            }
            
            Debug.Log($"Attempting Firebase Auth login with email: {email}");
            
            // Sign in with email
            var result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
            var user = result.User;

            // Make sure email is verified
            if (!user.IsEmailVerified)
            {
                Debug.LogWarning("Email not verified. Please verify your email before logging in.");
                Auth.SignOut();
                return false;
            }

            Debug.Log("✅ Login successful!");
            return true;

        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e.ToString());
            return false;
        }
    }

    /// <summary>
    /// Fetches all quests from Firebase and returns them as runtime Quest ScriptableObjects
    /// </summary>
    /// <param name="onQuestsFetched">Callback invoked with the list of Quests</param>
    public void GetAllQuestsAsScriptableObjects(System.Action<List<Quest>> onQuestsFetched)
    {
        if (DbReference == null)
        {
            Debug.LogError("DbReference is null. Make sure Firebase is initialized.");
            onQuestsFetched?.Invoke(null);
            return;
        }

        DbReference.Child("quests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch quests: " + task.Exception);
                onQuestsFetched?.Invoke(null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<Quest> questList = new List<Quest>();

                foreach (DataSnapshot child in snapshot.Children)
                {
                    string json = child.GetRawJsonValue();
                    // Use a temporary class to parse JSON first
                    FirebaseQuestData data = JsonUtility.FromJson<FirebaseQuestData>(json);

                    Vector2 locationData = GetLocationFromCSV(data.location);
                    // Create runtime ScriptableObject Quest
                    Quest runtimeQuest = Quest.CreateRuntimeQuest(
                        child.Key,
                        data.title,
                        data.description,
                        locationData,   
                        data.xp
                    );

                    questList.Add(runtimeQuest);
                }

                onQuestsFetched?.Invoke(questList);
            }
        });
    }

    // Temporary class to parse Firebase JSON
    [System.Serializable]
    private class FirebaseQuestData
    {
        public string title;
        public string description;
        public string location;
        public int xp;
    }

    /// <summary>
    /// Gets the latitude and longitude of a building from BuildingLocations.csv in Resources
    /// </summary>
    /// <param name="locationName">Name of the building to search for</param>
    /// <returns>Vector2(latitude, longitude) or Vector2.zero if not found</returns>
    public static Vector2 GetLocationFromCSV(string locationName)
    {
        // Load the CSV file from Resources
        TextAsset csvFile = Resources.Load<TextAsset>("BuildingLocations");
        if (csvFile == null)
        {
            Debug.LogError("BuildingLocations.csv not found in Resources!");
            return Vector2.zero;
        }

        StringReader reader = new StringReader(csvFile.text);
        string line;

        // Read the header line
        line = reader.ReadLine();
        if (line == null)
        {
            Debug.LogError("CSV file is empty!");
            return Vector2.zero;
        }

        string[] headers = line.Split(',');
        int buildingIndex = Array.IndexOf(headers, "Building");
        int latIndex = Array.IndexOf(headers, "Latitude");
        int lonIndex = Array.IndexOf(headers, "Longitude");

        if (buildingIndex == -1 || latIndex == -1 || lonIndex == -1)
        {
            Debug.LogError("CSV missing required columns (Building, Latitude, Longitude)!");
            return Vector2.zero;
        }

        // Read each line
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');

            if (values.Length <= Mathf.Max(buildingIndex, latIndex, lonIndex))
                continue; // skip invalid lines

            if (values[buildingIndex].Trim() == locationName)
            {
                if (float.TryParse(values[latIndex], out float lat) &&
                    float.TryParse(values[lonIndex], out float lon))
                {
                    return new Vector2(lat, lon);
                }
                else
                {
                    Debug.LogError($"Invalid latitude/longitude for {locationName}");
                    return Vector2.zero;
                }
            }
        }

        Debug.LogWarning($"Location '{locationName}' not found in CSV!");
        return Vector2.zero;
    }



    // Unity OnDestroy - Clean up event subscriptions to prevent memory leaks
    private void OnDestroy()
    {
        // Unsubscribe from Firebase auth events
        if (Auth != null)
        {
            Auth.StateChanged -= OnAuthStateChanged;
        }
    }
}