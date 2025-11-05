using UnityEngine;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var exitButton = root.Q<Button>("exitButton");

        exitButton.clicked += ExitUI;
    }

    private void ExitUI()
    {
        gameObject.SetActive(false);

    }
}
