using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Mathematics;

public class ButtonController : MonoBehaviour
{
    [Header("Screen References")]
    
    [SerializeField] private GameObject AuthScreen; // Reference to the Sign-Up panel
    [SerializeField] private GameObject ProfileScreen;
    [SerializeField] private GameObject CharacterCustomizationScreen;
    [SerializeField] private GameObject MainScreen;
    [SerializeField] private GameObject Map;
    [SerializeField] private GameObject ARScreen;
    [SerializeField] private GameObject SettingsScreen;
    [SerializeField] private GameObject ShopScreen;
    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private GameObject QuestScreen;

    private void PlayButtonSound()
    {

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonSound);
    }

    void OnEnable()
    {
        // Main Screen
        var root = MainScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
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

        // Profile Screen
        root = ProfileScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            var exitButton = root.Q<Button>("ExitButton");
            if (exitButton == null) Debug.Log("ExitButton button not found!");
            else exitButton.clicked += OnExitButton;


            var customizeButton = root.Q<Button>("CustomizeButton");
            if (customizeButton == null) Debug.Log("CustomizeButton button not found!");
            else customizeButton.clicked += OnCustomizeButton;
        }

        // Character Customization Screen
        root = CharacterCustomizationScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            Debug.Log("Adding Character Customization screen button handlers");
            var backButton = root.Q<Button>("BackButton");
            if (backButton == null) Debug.Log("BackButton button not found!");
            else backButton.clicked += OnBackButton;
        }

        // AR Screen
        root = ARScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            root.RegisterCallback<ClickEvent>(evt =>
            {
                Debug.Log("Clicked: " + evt.target);
            });
            Debug.Log("Adding AR screen button handlers");
            // Add AR screen button handlers here if needed
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

            var customizeButton = root.Q<Button>("customizeButton");
            if (customizeButton == null) Debug.Log("customizeButton button not found!");
            else customizeButton.clicked += OnMenuToCustomizeButton;

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

            var logoutButton = root.Q<Button>("logoutButton");
            if (logoutButton == null) Debug.Log("logoutButton button not found!");
            else logoutButton.clicked += OnLogout;

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
    private IEnumerator DelayedScreenChange(GameObject[] toEnable, GameObject[] toDisable)
    {
        PlayButtonSound();
        foreach (var obj in toEnable)
        {
            obj.SetActive(true);
            PlayScreenAnimation(obj);
        }
        foreach (var obj in toDisable)
            PlayDisableScreenAnimation(obj);
        
        yield return new WaitForSeconds(0.3f);

        foreach (var obj in toDisable)
            obj.SetActive(false);
        
        OnEnable(); // rebind UI Toolkit buttons
    }

    private void PlayScreenAnimation(GameObject screenObj)
    {
        var UIDoc = screenObj.GetComponentInChildren<UIDocument>();
        if (UIDoc == null)
            return;
        var root = UIDoc.rootVisualElement;
        if (root == null) return;

        Debug.Log("Animating screen: " + screenObj.name);

        // Remove any previous "show" state
        root.RemoveFromClassList("show");
        root.AddToClassList("screen");

        // Delay 1 frame so transitions can fire
        root.schedule.Execute(() =>
        {
            root.AddToClassList("show");
            Debug.Log("Showing Screen now");
        }).ExecuteLater(10);
    }
    private void PlayDisableScreenAnimation(GameObject screenObj)
    {
        var UIDoc = screenObj.GetComponentInChildren<UIDocument>();
        if (UIDoc == null)
            return;
        var root = UIDoc.rootVisualElement;
        if (root == null) return;

        Debug.Log("Animating screen: " + screenObj.name);

        // Remove any previous "show" state
        root.RemoveFromClassList("show");

    }


    private void OnLogout()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {AuthScreen},
            new GameObject[] {SettingsScreen}
        ));
    }
    private void OnARBackButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {ARScreen}
        ));
    }

    private void OnCameraButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {ARScreen},
            new GameObject[] {MenuScreen}
        ));
    }
    private void OnMenuButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MenuScreen}, 
            new GameObject[] {MainScreen, Map}
        ));
    }
    private void OnExitMenuButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {MenuScreen}
        ));
    }
    private void OnProfileButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {ProfileScreen},
            new GameObject[] {MainScreen, Map}
        ));
    }

    private void OnSettingsButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {SettingsScreen},
            new GameObject[] {MainScreen, Map}
        ));
    }
    private void OnCloseSettings()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {SettingsScreen}
        ));
    }
    private void OnMenuToQuest()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {QuestScreen},
            new GameObject[] {MenuScreen}
        ));
    }
    private void OnShopButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {ShopScreen},
            new GameObject[] {MenuScreen}
        ));
    }
    private void OnCloseShop()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {ShopScreen}
        ));
    }

    private void OnExitButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {ProfileScreen}
        ));
    }
    private void OnMenuToCustomizeButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {CharacterCustomizationScreen},
            new GameObject[] {MenuScreen}
        ));
    }
    private void OnCustomizeButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {CharacterCustomizationScreen},
            new GameObject[] {ProfileScreen}
        ));
    }
    private void OnBackButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {ProfileScreen},
            new GameObject[] {CharacterCustomizationScreen}
        ));
    }

    private void OnQuestButton()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {QuestScreen},
            new GameObject[] {MainScreen, Map}
        ));
    }
    // For Canvas-based Quest Screen
    public void OnCloseQuest()
    {
        StartCoroutine(DelayedScreenChange(
            new GameObject[] {MainScreen, Map},
            new GameObject[] {QuestScreen}
        ));
    }

}