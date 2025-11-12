using UnityEngine;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private GameObject ProfileScreen;
    [SerializeField] private GameObject CharacterCustomizationScreen;
    [SerializeField] private GameObject MainScreen;
    [SerializeField] private GameObject Map;
    [SerializeField] private GameObject ARScreen;
    [SerializeField] private GameObject SettingsScreen;


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
    }
    private void OnARBackButton()
    {
        Debug.Log("AR Back Button Pressed");
        ARScreen.SetActive(false);
        MainScreen.SetActive(true);
        Map.SetActive(true);
        OnEnable();
    }
    private void OnMenuButton()
    {
        ARScreen.SetActive(true);
        MainScreen.SetActive(false);
        Map.SetActive(false);
        OnEnable();
    }
    private void OnProfileButton()
    {
        ProfileScreen.SetActive(true);
        MainScreen.SetActive(false);
        Map.SetActive(false);
        OnEnable();
    }
    private void OnExitButton()
    {
        ProfileScreen.SetActive(false);
        MainScreen.SetActive(true);
        Map.SetActive(true);
        OnEnable();
    }
    private void OnCustomizeButton()
    {
        ProfileScreen.SetActive(false);
        CharacterCustomizationScreen.SetActive(true);
        OnEnable();
    }
    private void OnBackButton()
    {
        CharacterCustomizationScreen.SetActive(false);
        ProfileScreen.SetActive(true);
        OnEnable();
    }

}
