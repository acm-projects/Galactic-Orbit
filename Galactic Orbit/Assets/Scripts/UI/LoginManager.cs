using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.AI;
public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public TMP_Text loginTitle;

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
    }

    private async void OnLoginButtonClicked()
    {
        if (emailField == null) Debug.LogError("emailField is null!");
        if (passwordField == null) Debug.LogError("passwordField is null!");
        if (loginButton == null) Debug.LogError("loginButton is null!");
        if (AuthManager.Instance == null) Debug.LogError("AuthManager.Instance is null!");

        string username = emailField.text;
        string password = passwordField.text;

        loginButton.interactable = false;

        bool success = await AuthManager.Instance.Login(username, password);

        if (success)
        {
            Debug.Log("Login successful.");
            // bring to next scene
        }
        else
        {
            Debug.Log("Login failed.");
            loginButton.interactable = true;
        }
    }
}
