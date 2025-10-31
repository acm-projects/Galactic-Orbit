using UnityEngine;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private GameObject ProfileScreen;
    [SerializeField] private GameObject CharacterCustomizationScreen;
    [SerializeField] private GameObject MainScreen;
    [SerializeField] private GameObject ARScreen;


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
            var backButton = root.Q<Button>("BackButton");
            if (backButton == null) Debug.Log("BackButton button not found!");
            else backButton.clicked += OnBackButton;
        }
    }
    void OnDisable()
    {
        // Main Screen
        var root = MainScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            var profileButton = root.Q<Button>("ProfileButton");
            if (profileButton == null) Debug.Log("ProfileButton button not found!");
            else profileButton.clicked -= OnProfileButton;

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
            else exitButton.clicked -= OnExitButton;


            var customizeButton = root.Q<Button>("CustomizeButton");
            if (customizeButton == null) Debug.Log("CustomizeButton button not found!");
            else customizeButton.clicked -= OnCustomizeButton;
        }

        // Character Customization Screen
        root = CharacterCustomizationScreen.GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root != null)
        {
            var backButton = root.Q<Button>("BackButton");
            if (backButton == null) Debug.Log("BackButton button not found!");
            else backButton.clicked -= OnBackButton;
        }
    }
    private void OnMenuButton()
    {
        SimpleARController.Instance.ToggleARMode();
        ARScreen.SetActive(true);
        MainScreen.SetActive(false);
    }
    private void OnProfileButton()
    {
        ProfileScreen.SetActive(true);
        MainScreen.SetActive(false);
        OnEnable();
    }
    private void OnExitButton()
    {
        ProfileScreen.SetActive(false);
        MainScreen.SetActive(true);
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
