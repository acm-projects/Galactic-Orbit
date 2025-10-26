using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private string persistentScene = "Managers";
    [SerializeField] private GameObject loadingCanvas;

    private string currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Start()
    {
        LoadLevel("AuthScene");
    }

    public void LoadLevel(string newSceneName)
    {
        StartCoroutine(SwitchLevelWithLoading(newSceneName));
    }

    private IEnumerator SwitchLevelWithLoading(string newSceneName)
    {
        // ✅ Step 1: Load the loading scene additively and wait for activation
        loadingCanvas.SetActive(true);

        // Wait one frame so UI fully renders (important!)
        yield return null;

        // ✅ Step 2: Unload the current level
        if (!string.IsNullOrEmpty(currentLevel))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentLevel);
            while (!unloadOp.isDone)
                yield return null;
        }

        // ✅ Step 3: Load the new scene additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
        {
            // Optional: update progress bar here
            yield return null;
        }

        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        SceneManager.SetActiveScene(newScene);
        currentLevel = newSceneName;

        // ✅ Step 4: Unload the loading scene
        loadingCanvas.SetActive(false);
    }
}
