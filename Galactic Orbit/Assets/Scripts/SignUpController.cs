using UnityEngine;
using UnityEngine.UIElements;

public class SignUpController : MonoBehaviour
{
    private TextField emailField;
    private TextField usernameField;
    private TextField passwordField;
    private Button submitButton;

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
        if (submitButton ==null) Debug.LogError("SubmitButton button not found!");

        // Hook up the button’s clicked event
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

        // Check username availability first
        UserProfileManager.Instance.IsUsernameTaken(username, (isTaken) =>
        {
            if (isTaken)
            {
                Debug.Log("Username is already taken");
                return;
            }

            // Register the user - use the actual method name and signature
            AuthenticationManager.Instance.RegisterUser(email, password, username, username, (success, message) =>
            {
                if (success)
                {
                    Debug.Log("Registration successful: " + message);
                    // TODO: Navigate to login or main scene
                }
                else
                {
                    Debug.LogError("Registration failed: " + message);
                }
            });
        });
    }
}
