using UnityEngine;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var exitButton = root.Q<Button>("closeButton");

        exitButton.clicked += ExitUI;
    }

    private void ExitUI()
    {
        gameObject.SetActive(false);

    }
}
