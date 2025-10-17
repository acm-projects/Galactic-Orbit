using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class SignUpController : MonoBehaviour
{
    private TextField emailField;
    private TextField usernameField;
    private TextField passwordField;
    private Button submitButton;
    public GameObject LogInPanel;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Get references to inputs and button
        emailField = root.Q<TextField>("EmailInput");
        usernameField = root.Q<TextField>("UsernameInput");
        passwordField = root.Q<TextField>("PasswordInput");
        submitButton = root.Q<Button>("SubmitButton");

        if (emailField ==null) Debug.LogError("EmailInput field not found!");
        if (usernameField ==null) Debug.LogError("UsernameInput field not found!");
        if (passwordField ==null) Debug.LogError("PasswordInput field not found!");
        if (submitButton == null) Debug.LogError("SubmitButton button not found!");

        // Hook up the button’s clicked event
        
        var switchButton = root.Q<Button>("SwitchScreenButton");
        switchButton.clicked += switchPanels;

        submitButton.clicked += OnSubmit;
    }

    private void OnSubmit()
    {
        string email = emailField.value;
        string username = usernameField.value;
        string password = passwordField.value;

        emailField.value = "";
        usernameField.value = "";
        passwordField.value = "";

        Debug.Log($"Submitted! Email: {email}, Username: {username}, Password: {password}");
        StartCoroutine(SubmitSection());
        // Here you could add validation, send to server, etc.
    }
    
     private void switchPanels()
    {
        LogInPanel.SetActive(true);
        gameObject.SetActive(false);
    }
    IEnumerator SubmitSection()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var colorSection = root.Q<VisualElement>("Pressing");
        colorSection.AddToClassList("selected");

        yield return new WaitForSeconds(0.5f);

        colorSection.RemoveFromClassList("selected");

    }
}
