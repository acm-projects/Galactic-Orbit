using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;

/// <summary>
/// Controls the Profile Screen UI and loads real user data from Firebase
/// Attach this to the GameObject that has the UIDocument component
/// </summary>
public class MainScreenDataUpdater : MonoBehaviour
{
    private UIDocument uiDocument;
    
    // UI Elements
    private Label levelLabel;
    private ProgressBar experienceBar;
    private ProgressBar questBar;
    private Label questProgressLabel;

    private bool isInitialized = false;

    void OnEnable()
    {
        // Use coroutine to ensure everything is ready
        StartCoroutine(InitializeUI());
    }

    public IEnumerator InitializeUI()
    {
        Debug.Log($"🟣 InitializeUI() started on {gameObject.name}");
        yield return null;
        Debug.Log("✅ UI coroutine reached post-yield");

        // Wait a frame to ensure UIDocument is ready
        if (isInitialized)
            yield break;

        // Wait one extra frame to ensure UI Toolkit is ready
        yield return null;
        yield return null;

        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument not found! Attach this script to the GameObject with UIDocument.");
            yield break;
        }

        // Get root visual element
        var root = uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("❌ Root visual element is null on " + gameObject.name);
            // Debug.LogError("Root visual element is null!");
            yield break;
        }
        else
        {
            Debug.Log("✅ Root visual element found, child count: " + root.childCount);
        }

        // Find all UI elements with null checks
        try
        {
            levelLabel = root.Q<Label>("LevelNumber");
            
            experienceBar = root.Q<ProgressBar>("XPBar");
            if (experienceBar != null)
            {
                Debug.Log($"ProgressBar found. Children count: {experienceBar.childCount}");
            }

            questProgressLabel = root.Q<Label>("QuestCompletion");

            questBar = root.Q<ProgressBar>("QuestCompletionBar");
            
            // Buttons
            //exitButton = root.Q<Button>("ExitButton");
            //customizeButton = root.Q<Button>("CustomizeButton");

            // Setup button listeners
            //if (exitButton != null)
            //    exitButton.clicked += OnExitClicked;
            
            //if (customizeButton != null)
            //    customizeButton.clicked += OnCustomizeClicked;

            //Debug.Log("UI elements found successfully");
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error finding UI elements: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            yield break;
        }

        // Wait for managers to be ready
        yield return StartCoroutine(WaitForManagersAndLoadProfile());
    }

    IEnumerator WaitForManagersAndLoadProfile()
    {
        // Wait for FirebaseManager to be ready
        int maxWaitFrames = 300; // 5 seconds at 60fps
        int frameCount = 0;
        
        Debug.Log("=== Waiting for managers ===");
        
        while (frameCount < maxWaitFrames)
        {
            bool firebaseExists = FirebaseManager.Instance != null;
            bool userLoggedIn = firebaseExists && FirebaseManager.Instance.CurrentUser != null;
            bool profileManagerExists = UserProfileManager.Instance != null;
            
            if (frameCount % 60 == 0) // Log every second
            {
                Debug.Log($"Frame {frameCount}: Firebase={firebaseExists}, User={userLoggedIn}, ProfileMgr={profileManagerExists}");
            }
            
            if (firebaseExists && userLoggedIn && profileManagerExists)
            {
                Debug.Log("✅ All managers ready, loading profile...");
                break;
            }
            
            frameCount++;
            yield return null;
        }

        if (frameCount >= maxWaitFrames)
        {
            Debug.LogError("⚠️ TIMEOUT waiting for managers!");
            Debug.LogError($"FirebaseManager exists: {FirebaseManager.Instance != null}");
            Debug.LogError($"User logged in: {FirebaseManager.Instance?.CurrentUser != null}");
            Debug.LogError($"UserProfileManager exists: {UserProfileManager.Instance != null}");
            Debug.LogError("CHECK: Is UserProfileManager GameObject in Managers scene?");
            ShowTestData();
            yield break;
        }

        // Load user data
        LoadUserProfile();
    }

    void OnDisable()
    {

        isInitialized = false;
    }

    public void Reinitialize()
    {
        Debug.Log($"🔁 Reinitialize() called on {gameObject.name}");
        StopAllCoroutines();
        isInitialized = false;
        StartCoroutine(InitializeUI());
    }

    /// <summary>
    /// Load user profile data from Firebase and update UI
    /// </summary>
    void LoadUserProfile()
    {
        Debug.Log("=== LoadUserProfile called ===");
        
        // Check if user is logged in
        if (FirebaseManager.Instance == null)
        {
            Debug.LogWarning("FirebaseManager.Instance is null");
            ShowTestData();
            return;
        }

        if (FirebaseManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("FirebaseManager.Instance.CurrentUser is null - showing test data");
            ShowTestData();
            return;
        }

        Debug.Log($"Current user: {FirebaseManager.Instance.CurrentUser.UserId}");

        // Check if UserProfileManager exists
        if (UserProfileManager.Instance == null)
        {
            Debug.LogError("UserProfileManager.Instance is null!");
            ShowTestData();
            return;
        }

        Debug.Log("Calling GetCurrentUserProfile...");

        // Load profile from Firebase
        UserProfileManager.Instance.GetCurrentUserProfile((profile) =>
        {
            Debug.Log("=== GetCurrentUserProfile callback executed ===");
            
            if (profile != null)
            {
                Debug.Log($"Profile loaded: {profile.username}, Level: {profile.level}, XP: {profile.totalPoints}");
                UpdateUIWithProfile(profile);
            }
            else
            {
                Debug.LogError("Profile is null in callback");
                ShowTestData();
            }
        });

        Debug.Log("GetCurrentUserProfile call completed (waiting for callback)");
    }

    /// <summary>
    /// Update all UI elements with real user data
    /// </summary>
    void UpdateUIWithProfile(UserProfile profile)
    {
        Debug.Log($"=== UpdateUIWithProfile called ===");
        Debug.Log($"Profile: Level {profile.level}, {profile.totalPoints} XP, {profile.coins} coins, {profile.items} items");

        if (!isInitialized)
        {
            Debug.LogError("UI not initialized yet!");
            return;
        }

        // Level
        if (levelLabel != null)
        {
            levelLabel.text = profile.level.ToString();
            Debug.Log($"✅ Set level to: {levelLabel.text}");
        }
        else
        {
            Debug.LogWarning("❌ levelLabel is null");
        }

        // Experience/Points
        if (experienceBar != null)
        {
            // Calculate experience for current level
            int currentLevelXP = (profile.level - 1) * 100;
            int nextLevelXP = profile.level * 100;
            int xpInCurrentLevel = profile.totalPoints - currentLevelXP;
            int xpNeededForLevel = nextLevelXP - currentLevelXP;

            // Update progress bar (0-100 scale)
            float progress = (float)xpInCurrentLevel / xpNeededForLevel * 100f;
            experienceBar.value = progress;
        }
        else
        {
            Debug.LogWarning("❌ experienceBar is null");
        }

        // Quest Completion
        if (questBar != null && QuestManager.Instance != null)
        {
            int activeQuestsCount = QuestManager.Instance.activeQuests.Count;
            float maxQuests = 5; //Hardcoded
            questBar.value = (maxQuests-activeQuestsCount) / maxQuests;

        }

        if (questProgressLabel != null && QuestManager.Instance != null)
        {
            int activeQuestsCount = QuestManager.Instance.activeQuests.Count;
            float maxQuests = 5; //Hardcoded
            questProgressLabel.text = $"{maxQuests-activeQuestsCount}/{maxQuests}";
        }

        Debug.Log($"✅✅✅ Profile UI updated successfully ✅✅✅");
    }

    /// <summary>
    /// Show test data when user is not logged in (for testing UI)
    /// </summary>
    void ShowTestData()
    {
        Debug.Log("=== ShowTestData called ===");
        
        if (levelLabel != null)
        {
            levelLabel.text = "3";
            Debug.Log($"Set level to: {levelLabel.text}");
        }

        if (experienceBar != null)
        {
            experienceBar.value = 37f;
            Debug.Log($"Set experience bar to: {experienceBar.value}");
        }
/*
        if (experienceText != null)
        {
            experienceText.text = "120/324";
            Debug.Log($"Set experience text to: {experienceText.text}");
        }

        if (coinsLabel != null)
            coinsLabel.text = "24";

        if (itemsLabel != null)
            itemsLabel.text = "4";

        if (locationsVisitedLabel != null)
            locationsVisitedLabel.text = "3";

        if (distanceLabel != null)
            distanceLabel.text = "6.7 miles";

        if (friendsLabel != null)
            friendsLabel.text = "5";*/

        Debug.Log("⚠️ Showing test data (user not logged in or managers not ready)");
    }


  
}