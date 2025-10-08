using UnityEngine;
using UnityEngine.UIElements;

public class SignUpController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject loginPanelGO;   // assign LoginMenuPanel in Inspector
    [SerializeField] private GameObject signupPanelGO;  // assign SignupMenuPanel in Inspector

    private Button switchToLoginButton;  // reference for the "Log In" button

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

        if (emailField == null) Debug.LogError("EmailInput field not found!");
        if (usernameField == null) Debug.LogError("UsernameInput field not found!");
        if (passwordField == null) Debug.LogError("PasswordInput field not found!");
        if (submitButton == null) Debug.LogError("SubmitButton button not found!");

        // Hook up the button’s clicked event
        submitButton.clicked += OnSubmit;

        // Query the "Log In" button
        switchToLoginButton = root.Q<Button>("SwitchToLoginButton");

        if (switchToLoginButton != null)
        {
            switchToLoginButton.clicked += OnSwitchToLogin;
        }
        else
        {
            Debug.LogError("SwitchToLoginButton not found in SignupMenuUI!");
        }

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
        UserProfileManager.Instance.IsUsernameTaken(username, async (isTaken) =>
        {
            if (isTaken)
            {
                Debug.Log("Username is already taken");
                return;
            }

            // Register the user
            bool success = await AuthenticationManager.Instance.RegisterAsync(email, password, username, username);

            if (success)
            {
                Debug.Log("Registration successful!");
                // Navigate to login or main scene
            }
            else
            {
                Debug.Log("Registration failed.");
            }
        });
    }

    private void OnSwitchToLogin()
    {
        if (signupPanelGO != null && loginPanelGO != null)
        {
            signupPanelGO.SetActive(false);  // hide signup panel
            loginPanelGO.SetActive(true);    // show login panel
        }
        else
        {
            Debug.LogWarning("LoginPanelGO or SignupPanelGO not assigned in Inspector!");
        }
    }
    
    void OnDisable()
    {
        if (submitButton != null)
            submitButton.clicked -= OnSubmit;

        if (switchToLoginButton != null)
            switchToLoginButton.clicked -= OnSwitchToLogin;
    }
}
