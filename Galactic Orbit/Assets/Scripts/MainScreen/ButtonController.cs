using UnityEngine;
using UnityEngine.UIElements;

public class ButtonController : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private GameObject ProfileScreen;
    [SerializeField] private GameObject CharacterCustomizationScreen;
    [SerializeField] private GameObject MainScreen;
    

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Main Screen
        var profileButton = root.Q<Button>("ProfileButton");
        if (profileButton == null) Debug.Log("ProfileButton button not found!");
        else profileButton.clicked += OnProfileButton;

        // Profile Screen
        var exitButton = root.Q<Button>("ExitButton");
        if (exitButton == null) Debug.Log("ExitButton button not found!");
        else exitButton.clicked += OnExitButton;

        var customizeButton = root.Q<Button>("CustomizeButton");
        if (customizeButton == null) Debug.Log("CustomizeButton button not found!");
        else customizeButton.clicked += OnCustomizeButton;

        // Character Customization Screen
        var backButton = root.Q<Button>("BackButton");
        if (backButton == null) Debug.Log("BackButton button not found!");
        else backButton.clicked += OnBackButton;

    }

    private void OnProfileButton()
    {
        ProfileScreen.SetActive(true);
        MainScreen.SetActive(false);
    }
    private void OnExitButton()
    {
        ProfileScreen.SetActive(false);
        MainScreen.SetActive(true);
    }
    private void OnCustomizeButton()
    {
        ProfileScreen.SetActive(false);
        CharacterCustomizationScreen.SetActive(true);
    }
    private void OnBackButton()
    {
        CharacterCustomizationScreen.SetActive(false);
        ProfileScreen.SetActive(true);
    }

}
