using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
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
        // Singleton pattern: destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);

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
            
            // Check if input looks like an email (contains @)
            if (!emailOrUsername.Contains("@"))
            {
                // It's a username - look up the email from user profile
                var snapshot = await DbReference.Child("userProfiles")
                    .OrderByChild("username")
                    .EqualTo(emailOrUsername)
                    .GetValueAsync();
                
                if (snapshot.Exists && snapshot.ChildrenCount > 0)
                {
                    // Get the email from the profile
                    foreach (var child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        UserProfile profile = JsonUtility.FromJson<UserProfile>(json);
                        email = profile.email;
                        break;
                    }
                }
                else
                {
                    Debug.LogError("Username not found");
                    return false;
                }
            }
            
            // Sign in with email
            var result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
            return false;
        }
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