using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [SerializeField] private GameObject SettingsScreen;
    [SerializeField] private GameObject ShopScreen;
    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private GameObject QuestScreen;


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
            if (profileButton == null) Debug.Log("ProfileButton button not found!");
            else profileButton.clicked += OnProfileButton;

            var MenuButton = root.Q<Button>("MenuButton");
            if (MenuButton == null) Debug.Log("MenuButton button not found!");
            else MenuButton.clicked += OnMenuButton;

            var SettingsButton = root.Q<Button>("SettingsButton");
            if (SettingsButton == null) Debug.Log("SettingsButton button not found!");
            else SettingsButton.clicked += OnSettingsButton;

            var QuestButton = root.Q<Button>("QuestButton");
            if (QuestButton == null) Debug.Log("QuestButton button not found!");
            else QuestButton.clicked += OnQuestButton;
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
            if (arBackButton == null) Debug.Log("ARBackButton button not found!");
            else arBackButton.clicked += OnARBackButton;
        }

        // Menu Screen
        root = MenuScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            Debug.Log("Adding Menu screen button handlers");
            var exitMenuButton = root.Q<Button>("exitButton");
            if (exitMenuButton == null) Debug.Log("exitButton button not found!");
            else exitMenuButton.clicked += OnExitMenuButton;

            var cameraButton = root.Q<Button>("cameraButton");
            if (cameraButton == null) Debug.Log("cameraButton button not found!");
            else cameraButton.clicked += OnCameraButton;

            var shopButton = root.Q<Button>("shopButton");
            if (shopButton == null) Debug.Log("shopButton button not found!");
            else shopButton.clicked += OnShopButton;

            var questButton = root.Q<Button>("questButton");
            if (questButton == null) Debug.Log("questButton button not found!");
            else questButton.clicked += OnMenuToQuest;
        }

        // Settings Screen
        root = SettingsScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            Debug.Log("Adding Settings screen button handlers");
            // Add Settings screen button handlers here if needed
            var closeButton = root.Q<Button>("closeButton");
            if (closeButton == null) Debug.Log("closeButton button not found!");
            else closeButton.clicked += OnCloseSettings;

        }

        // Shop Screen
        root = ShopScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            Debug.Log("Adding Shop screen button handlers");
            // Add Shop screen button handlers here if needed
            var closeShopButton = root.Q<Button>("closeButton");
            if (closeShopButton == null) Debug.Log("closeButton button not found!");
            else closeShopButton.clicked += OnCloseShop;
        }
    }
    private IEnumerator DelayedScreenChange(System.Action action)
    {
        yield return new WaitForSeconds(0.2f); // 200ms delay
        action?.Invoke();
        OnEnable(); // rebind UI Toolkit buttons
    }

    private void OnARBackButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            Debug.Log("AR Back Button Pressed");
            ARScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
            OnEnable();
        }));
    }

    private void OnCameraButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ARScreen.SetActive(true);
            MenuScreen.SetActive(false);
        }));
    }

    void OnMenuButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            MenuScreen.SetActive(true);
            MainScreen.SetActive(false);
            Map.SetActive(false);
        }));
    }
    private void OnExitMenuButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            MenuScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
        }));
    }
    private void OnProfileButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ProfileScreen.SetActive(true);
            MainScreen.SetActive(false);
            Map.SetActive(false);
        }));
    }

    private void OnSettingsButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            SettingsScreen.SetActive(true);
            MainScreen.SetActive(false);
            Map.SetActive(false);
        }));
    }
    private void OnCloseSettings()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            SettingsScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
        }));
    }
    private void OnMenuToQuest()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            MenuScreen.SetActive(false);
            QuestScreen.SetActive(true);
        }));
    }
    private void OnShopButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ShopScreen.SetActive(true);
            MenuScreen.SetActive(false);
        }));
    }
    private void OnCloseShop()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ShopScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
        }));
    }

    private void OnExitButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ProfileScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
        }));
    }
    private void OnCustomizeButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            ProfileScreen.SetActive(false);
            CharacterCustomizationScreen.SetActive(true);
        }));
    }
    private void OnBackButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            CharacterCustomizationScreen.SetActive(false);
            ProfileScreen.SetActive(true);
        }));
    }

    private void OnQuestButton()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            QuestScreen.SetActive(true);
            MainScreen.SetActive(false);
            Map.SetActive(false);
        }));
    }
    // For Canvas-based Quest Screen
    public void OnCloseQuest()
    {
        StartCoroutine(DelayedScreenChange(() =>
        {
            QuestScreen.SetActive(false);
            MainScreen.SetActive(true);
            Map.SetActive(true);
        }));
    }

}
