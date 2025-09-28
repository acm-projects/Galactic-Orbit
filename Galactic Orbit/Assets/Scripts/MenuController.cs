using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the main menu: handles menu buttons, their selection states,
/// and dynamically populates sub-items when a menu option is selected.
/// </summary>
public class MenuController : MonoBehaviour
{

    public GameObject targetObject; // Target object to manipulate (if needed)
    // --- Fields ---
    private List<MenuOption> menuButtons;        // All top-level menu options
    private MenuOption selectedOption;           // Currently selected menu option
    private Dictionary<string, string[]> menuItems; // Mapping of menu option name -> sub-items

    private VisualElement itemContainer;         // Container that holds menu sub-items

    // --- Unity Lifecycle ---
    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Target object not assigned in the inspector.");
        }
    }

    private void OnEnable()
    {
        // Get root element from UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Initialize collections
        menuButtons = new List<MenuOption>();
        itemContainer = root.Q<VisualElement>("MenuItemsContainer");

        // Define menu items (could be loaded from external data later)
        menuItems = new Dictionary<string, string[]>
        {
            { "One",   new string[] { "1" } },
            { "Two",   new string[] { "1", "2" } },
            { "Three", new string[] { "1", "2", "3" } }
        };

        // Find all top-level buttons with class "menu-button"
        var buttons = root.Query<Button>(className: "menu-button").ToList();

        // Wrap each button into a MenuOption and hook events
        foreach (var button in buttons)
        {
            // Create a MenuOption for this button
            var option = new MenuOption(button.text, button, menuItems[button.text], targetObject);

            // Auto-select the first option by default
            if (selectedOption == null)
            {
                option.Select(itemContainer);
                selectedOption = option;
            }

            menuButtons.Add(option);

            // Attach click handler for the button
            button.clicked += () => OnMenuButtonClicked(option);
        }
    }

    // --- Event Handlers ---
    private void OnMenuButtonClicked(MenuOption option)
    {
        // Deselect current option and select the new one
        selectedOption?.Deselect();
        option.Select(itemContainer);
        selectedOption = option;
    }

    // --- Nested Classes ---

    /// <summary>
    /// Represents a top-level menu option.
    /// </summary>
    private class MenuOption
    {
        public string Name;                       // Name of the option
        public Button ButtonElement;              // UI Button for this option
        public List<MenuItem> Items;              // Sub-items belonging to this option
        public bool IsSelected;                   // Whether this option is currently selected
        public MenuItem SelectedItem;             // Currently selected sub-item

        public MenuOption(string name, Button buttonElement, string[] items, GameObject target)
        {
            Name = name;
            ButtonElement = buttonElement;
            Items = new List<MenuItem>();

            // Create sub-items as buttons
            foreach (var item in items)
            {
                var subButton = new Button { text = item };
                subButton.AddToClassList("menu-item");

                var menuItem = new MenuItem(item, subButton);

                // Auto-select the first sub-item
                if (SelectedItem == null)
                {
                    SelectedItem = menuItem;
                    menuItem.Select();
                }

                // Hook click event for sub-item
                subButton.clicked += () =>
                {
                    switch (Name)
                    {
                        case "One":
                            Debug.Log("Action for One - Item " + item);
                            break;
                        case "Two":
                            Debug.Log("Action for Two - Item " + item);
                            switch (item)
                            {
                                case "1":
                                    Debug.Log("Action for Two - Item 1");
                                    target.SetActive(true);
                                    break;
                                case "2":
                                    Debug.Log("Action for Two - Item 2");
                                    target.SetActive(false);
                                    break;
                            }
                            break;
                        case "Three":
                            switch (item)
                            {
                                case "1":
                                    Debug.Log("Action for Three - Item 1");
                                    target.transform.Rotate(0, 45, 0);
                                    break;
                                case "2":
                                    Debug.Log("Action for Three - Item 2");
                                    target.transform.Rotate(45, 0, 0);
                                    break;
                                case "3":
                                    Debug.Log("Action for Three - Item 3");
                                    target.transform.Rotate(0, 0, 45);
                                    break;
                            }
                            Debug.Log("Action for Three - Item " + item);
                            break;
                    }
                    SelectedItem?.Deselect();
                    SelectedItem = menuItem;
                    menuItem.Select();
                };

                Items.Add(menuItem);
            }

            IsSelected = false;
        }

        /// <summary>
        /// Select this menu option and display its sub-items.
        /// </summary>
        public void Select(VisualElement itemContainer)
        {
            Debug.Log("Selecting " + Name);
            IsSelected = true;

            ButtonElement.AddToClassList("selected");
            ButtonElement.RemoveFromClassList("unselected");

            // Clear previous items and display this option's sub-items
            itemContainer.Clear();
            foreach (var item in Items)
            {
                itemContainer.Add(item.ButtonElement);
            }
        }

        /// <summary>
        /// Deselect this menu option.
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;
            ButtonElement.RemoveFromClassList("selected");
            ButtonElement.AddToClassList("unselected");
        }
    }

    /// <summary>
    /// Represents a sub-item within a MenuOption.
    /// </summary>
    private class MenuItem
    {
        public string ID;                // Identifier of the sub-item
        public Button ButtonElement;     // UI Button for this sub-item
        public bool IsSelected;          // Whether this sub-item is selected

        public MenuItem(string name, Button buttonElement)
        {
            ID = name;
            ButtonElement = buttonElement;
            IsSelected = false;
        }

        public void Select()
        {
            IsSelected = true;
            ButtonElement.AddToClassList("item-selected");
        }

        public void Deselect()
        {
            IsSelected = false;
            ButtonElement.RemoveFromClassList("item-selected");
        }
    }
}
