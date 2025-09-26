using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    // Firebase variables
    public FirebaseAuth auth;
    public FirebaseUser user;

    // Persistant user data
    public string username;
    public int xp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            auth = FirebaseAuth.DefaultInstance;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.LoadScene("LoginScene");
    }

    // Log in user with Firebase
    public async Task<bool> Login(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
            return false;
        }
    }
}
