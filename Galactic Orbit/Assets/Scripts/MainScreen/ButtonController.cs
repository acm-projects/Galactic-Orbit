using UnityEngine;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    [Header("UI Panel References")]
    [SerializeField] private GameObject ProfileScreen;
    [SerializeField] private GameObject QuestScreen;
    [SerializeField] private GameObject EventScreen;
    [SerializeField] private GameObject SpecialScreen;
    [SerializeField] private GameObject CharacterCustomizationScreen;
    [SerializeField] private GameObject MainScreen;
    [SerializeField] private GameObject Map;
    [SerializeField] private GameObject ARScreen;

    void OnEnable()
    {
        SetupMainScreenButtons();
        SetupProfileScreenButtons();
        SetupCharacterCustomizationButtons();
        SetupARScreenButtons();
    }

    void SetupMainScreenButtons()
    {
        if (MainScreen == null) return;
        
        var uiDoc = MainScreen.GetComponentInChildren<UIDocument>();
        if (uiDoc == null) return;
        
        var root = uiDoc.rootVisualElement;
        if (root != null)
        {
            var profileButton = root.Q<Button>("ProfileButton");
            if (profileButton != null)
                profileButton.clicked += OnProfileButton;
            else
                Debug.LogWarning("ProfileButton not found in MainScreen");

            var menuButton = root.Q<Button>("MenuButton");
            if (menuButton != null)
                menuButton.clicked += OnMenuButton;
            else
                Debug.LogWarning("MenuButton not found in MainScreen");

            // ADD THIS:
            var questButton = root.Q<Button>("QuestButton");
            if (questButton != null)
                questButton.clicked += OnQuestButton;
            else
                Debug.LogWarning("QuestButton not found in MainScreen");
                
            // ADD THIS TOO:
            var settingsButton = root.Q<Button>("SettingsButton");
            if (settingsButton != null)
                settingsButton.clicked += OnSettingsButton;
            else
                Debug.LogWarning("SettingsButton not found in MainScreen");
        }
    }

    void SetupProfileScreenButtons()
{
    if (ProfileScreen == null)
    {
        Debug.LogError("ProfileScreen is null!");
        return;
    }
    
    // Try to find UIDocument anywhere in ProfileScreen's children
    var uiDocs = ProfileScreen.GetComponentsInChildren<UIDocument>(true);
    
    Debug.Log($"Found {uiDocs.Length} UIDocuments in ProfileScreen");
    
    foreach (var uiDoc in uiDocs)
    {
        if (uiDoc == null) continue;
        
        var root = uiDoc.rootVisualElement;
        if (root == null) continue;
        
        Debug.Log($"Checking UIDocument on {uiDoc.gameObject.name}");
        
        var exitButton = root.Q<Button>("ExitButton");
        if (exitButton != null)
        {
            Debug.Log("✅ Found ExitButton!");
            exitButton.clicked += OnProfileExitButton;
        }

        var customizeButton = root.Q<Button>("CustomizeButton");
        if (customizeButton != null)
        {
            Debug.Log("✅ Found CustomizeButton!");
            customizeButton.clicked += OnCustomizeButton;
        }
    }
}

    void SetupCharacterCustomizationButtons()
    {
        if (CharacterCustomizationScreen == null) return;
        
        var uiDoc = CharacterCustomizationScreen.GetComponentInChildren<UIDocument>();
        if (uiDoc == null) return;
        
        var root = uiDoc.rootVisualElement;
        if (root != null)
        {
            var backButton = root.Q<Button>("BackButton");
            if (backButton != null)
                backButton.clicked += OnBackButton;
        }
    }

    void SetupARScreenButtons()
    {
        if (ARScreen == null)
        {
            Debug.Log("ARScreen is null - skipping AR button setup");
            return;
        }
        
        var uiDocs = ARScreen.GetComponentsInChildren<UIDocument>(true);
        
        Debug.Log($"Found {uiDocs.Length} UIDocuments in ARScreen");
        
        foreach (var uiDoc in uiDocs)
        {
            if (uiDoc == null) continue;
            
            var root = uiDoc.rootVisualElement;
            if (root == null) continue;
            
            var arBackButton = root.Q<Button>("ARBackButton");
            if (arBackButton != null)
            {
                Debug.Log("✅ Found ARBackButton!");
                arBackButton.clicked += OnARBackButton;
            }
        }
    }

    // ===== BUTTON HANDLERS =====

    void OnProfileButton()
    {
        Debug.Log("Opening Profile");
        
        // Hide main screen
        if (MainScreen != null)
            MainScreen.SetActive(false);
        
        // Show profile
        if (ProfileScreen != null)
        {
            ProfileScreen.SetActive(true);
            
            // Reload profile data
            var profileController = ProfileScreen.GetComponentInChildren<UserProfileController>();
            if (profileController != null)
            {
                profileController.Reinitialize();
            }
        }
    }

    void OnProfileExitButton()
    {
        Debug.Log("Closing Profile");
        
        // Hide profile
        if (ProfileScreen != null)
            ProfileScreen.SetActive(false);
        
        // Show main screen
        if (MainScreen != null)
            MainScreen.SetActive(true);
        if (Map != null)
            Map.SetActive(true);
    }

    void OnMenuButton()
    {
        Debug.Log("Opening AR Menu");
        if (ARScreen != null)
            ARScreen.SetActive(true);
        if (MainScreen != null)
            MainScreen.SetActive(false);
        if (Map != null)
            Map.SetActive(false);
    }

    void OnARBackButton()
    {
        Debug.Log("AR Back Button Pressed");
        
        // Close AR mode properly
        if (SimpleARController.Instance != null)
        {
            SimpleARController.Instance.CloseARMode();
        }
        
        // Hide AR screen UI if it exists
        if (ARScreen != null)
            ARScreen.SetActive(false);
        
        // Show main screen
        if (MainScreen != null)
            MainScreen.SetActive(true);
    }

    void OnCustomizeButton()
    {
        Debug.Log("Opening Customization");
        if (ProfileScreen != null)
            ProfileScreen.SetActive(false);
        if (CharacterCustomizationScreen != null)
            CharacterCustomizationScreen.SetActive(true);
    }

    void OnBackButton()
    {
        Debug.Log("Closing Customization");
        if (CharacterCustomizationScreen != null)
            CharacterCustomizationScreen.SetActive(false);
        if (ProfileScreen != null)
            ProfileScreen.SetActive(true);
    }

    // ===== QUEST/EVENT HANDLERS (Add these to your UI buttons when ready) =====
    
    public void OnQuestButton()
    {
        Debug.Log("Opening Quests");
        if (MainScreen != null)
            MainScreen.SetActive(false);
        if (QuestScreen != null)
            QuestScreen.SetActive(true);
    }

    public void OnEventButton()
    {
        Debug.Log("Opening Events");
        if (MainScreen != null)
            MainScreen.SetActive(false);
        if (EventScreen != null)
            EventScreen.SetActive(true);
    }

    public void OnSpecialButton()
    {
        Debug.Log("Opening Specials");
        if (MainScreen != null)
            MainScreen.SetActive(false);
        if (SpecialScreen != null)
            SpecialScreen.SetActive(true);
    }
    
    public void CloseQuestScreen()
    {
        Debug.Log("Closing Quest Screen");
        if (QuestScreen != null)
            QuestScreen.SetActive(false);
        if (MainScreen != null)
            MainScreen.SetActive(true);
    }
    
    public void CloseEventScreen()
    {
        Debug.Log("Closing Event Screen");
        if (EventScreen != null)
            EventScreen.SetActive(false);
        if (MainScreen != null)
            MainScreen.SetActive(true);
    }

    public void CloseSpecialScreen()
    {
        Debug.Log("Closing Special Screen");
        if (SpecialScreen != null)
            SpecialScreen.SetActive(false);
        if (MainScreen != null)
            MainScreen.SetActive(true);
    }
    
    public void OnSettingsButton()
    {
        Debug.Log("Settings button clicked - not implemented yet");
        // TODO: Open settings screen when ready
    }
}