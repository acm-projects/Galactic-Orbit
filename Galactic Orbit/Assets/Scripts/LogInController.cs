using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.AI;

public class LogInController : MonoBehaviour
{
    private TextField usernameField;
    private TextField passwordField;
    private Button submitButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Get references to inputs and button
        usernameField = root.Q<TextField>("UsernameInput");
        passwordField = root.Q<TextField>("PasswordInput");
        submitButton = root.Q<Button>("SubmitButton");

        if (usernameField ==null) Debug.LogError("UsernameInput field not found!");
        if (passwordField ==null) Debug.LogError("PasswordInput field not found!");
        if (submitButton ==null) Debug.LogError("SubmitButton button not found!");

        // Hook up the button’s clicked event
        submitButton.clicked += OnSubmit;
    }

    private async void OnSubmit()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        usernameField.value = "";
        passwordField.value = "";

        Debug.Log($"Submitted! Username: {username}, Password: {password}");

        // Here you could add validation, send to server, etc.

        bool success = true; 
        //bool success = await AuthManager.Instance.Login(username, password);

        if (success)
        {
            Debug.Log("Login successful.");
            SceneManager.LoadScene("MapScene");
        }
        else
        {
            Debug.Log("Login failed.");
        }

    }
}
