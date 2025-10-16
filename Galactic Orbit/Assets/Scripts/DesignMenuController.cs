using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the main menu: handles menu buttons, their selection states,
/// and dynamically populates sub-items when a menu option is selected.
/// </summary>
public class DesignMenuController : MonoBehaviour
{
    public GameObject Character;
    public List<Material> PrimaryMaterial = new List<Material>();
    public List<Material> SecondaryMaterial = new List<Material>();
    public List<Material> TertiaryMaterial = new List<Material>();
    public List<Material> AccentMaterial_01 = new List<Material>();
    public List<Material> AccentMaterial_02 = new List<Material>();
    public List<Material> SkinColor = new List<Material>();
    

    // --- Fields ---
    private List<MenuOption> menuButtons;        // All top-level menu options
    private MenuOption selectedOption;           // Currently selected menu option
    private Dictionary<string, string[]> menuItems; // Mapping of menu option name -> sub-items

    // --- Unity Lifecycle ---
    private void Start()
    {
        if (PrimaryMaterial == null || SecondaryMaterial == null || TertiaryMaterial == null ||
            AccentMaterial_01 == null || AccentMaterial_02 == null)
        {
            Debug.LogWarning("Target Materials not assigned in the inspector.");
        }
    }

    private void OnEnable()
    {
        // Get root element from UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Initialize collections
        menuButtons = new List<MenuOption>();

        var renderers = Character.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            var mats = r.materials;
            foreach (var mat in mats)
            {
                if (mat.name.StartsWith("Primary"))
                {
                    PrimaryMaterial.Add(mat);
                }
                else if (mat.name.StartsWith("Secondary"))
                {
                    SecondaryMaterial.Add(mat);
                }
                else if (mat.name.StartsWith("Tertiary"))
                {
                    TertiaryMaterial.Add(mat);
                }
                else if (mat.name.StartsWith("Accent1"))
                {
                    AccentMaterial_01.Add(mat);
                }
                else if (mat.name.StartsWith("Accent2"))
                {
                    AccentMaterial_02.Add(mat);
                }
                else if (mat.name.StartsWith("SkinColor"))
                {
                    SkinColor.Add(mat);
                }
            }
        }

        // Define menu items (could be loaded from external data later)
        menuItems = new Dictionary<string, string[]>
        {
            { "Skin Color",
                new string[] { "rgb"} },
            { "Primary Color",
                new string[] { "rgb"} },
            { "Secondary Color",
                new string[] { "rgb"} },
            { "Tertiary Color",
                new string[] { "rgb"} },
            { "Accent Color 1",
                new string[] { "rgb"} },
            { "Accent Color 2",
                new string[] { "rgb"} },

        };

        // Wrap each button into a MenuOption and hook events
        foreach (var item in menuItems)
        {
            Debug.Log("Creating menu option: " + item.Key);
            // Create a MenuOption for this button
            var button = new Button { text = item.Key };
            button.AddToClassList("menu-button");
            button.AddToClassList("unselected");
            Debug.Log("Button created: " + menuItems[button.text]);
            
            List<Material> mat = null;
                switch (item)
                {
                    case var _ when item.Key.Contains("Primary"):
                        mat = PrimaryMaterial;
                        break;
                    case var _ when item.Key.Contains("Secondary"):
                        mat = SecondaryMaterial;
                        break;
                    case var _ when item.Key.Contains("Tertiary"):
                        mat = TertiaryMaterial;
                        break;
                    case var _ when item.Key.Contains("Accent Color 1"):
                        mat = AccentMaterial_01;
                        break;
                    case var _ when item.Key.Contains("Accent Color 2"):
                        mat = AccentMaterial_02;
                        break;
                    case var _ when item.Key.Contains("Skin Color"):
                        mat = SkinColor;
                        break;
            }
            var option = new MenuOption(button.text, button, menuItems[button.text], root, mat);

            // Auto-select the first option by default
            if (selectedOption == null)
            {
                option.Select();
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
        option.Select();
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
        public MenuItem SelectedItem;   
        public VisualElement root;
        public List<Material> Materials;
        public Color ReferenceColor;

        public MenuOption(string name, Button buttonElement, string[] items, VisualElement rootNode, List<Material> materials = null)
        {
            Name = name;
            ButtonElement = buttonElement;
            Items = new List<MenuItem>();
            root = rootNode;
            root.Q<VisualElement>("MenuTitleContainer").Add(buttonElement);

            Materials = materials;

            // Create sub-items as buttons
            foreach (var item in items)
            {
                var menuItem = new MenuItem();

                if (item.StartsWith("rgb"))
                {
                    var colorSelector = new RGBColorSelector(materials[0].color);
                    colorSelector.AddToClassList("rgb-color-selector");

                    var RSlider = colorSelector.rSlider;
                    var GSlider = colorSelector.gSlider;
                    var BSlider = colorSelector.bSlider;

                    menuItem.ID = item;
                    menuItem.Element = colorSelector;

                    // Changing Colors  
                    foreach (var mat in Materials)
                    {
                        RSlider.RegisterValueChangedCallback(evt =>
                        {
                            mat.color = new Color(RSlider.value, GSlider.value, BSlider.value);
                        });
                        GSlider.RegisterValueChangedCallback(evt =>
                        {
                            mat.color = new Color(RSlider.value, GSlider.value, BSlider.value);
                        });
                        BSlider.RegisterValueChangedCallback(evt =>
                        {
                            mat.color = new Color(RSlider.value, GSlider.value, BSlider.value);
                        });
                    }

                }
                else
                {
                    var subButton = new Button { text = item };
                    subButton.AddToClassList("menu-item");
                    menuItem.ID = item;
                    menuItem.Element = subButton;

                    // Hook click event for sub-item
                    subButton.clicked += () =>
                    {

                        DoMenuEvent(item);
                        SelectedItem?.Deselect();
                        SelectedItem = menuItem;
                        menuItem.Select();
                    };
                }
                // Auto-select the first sub-item
                if (SelectedItem == null)
                {
                    SelectedItem = menuItem;
                    menuItem.Select();
                }
                Items.Add(menuItem);
            }

            IsSelected = false;
        }
        public void DoMenuEvent(string item)
        {
            switch (item)
            {
                case "rgb":
                    Debug.Log("Action for One - Item " + item);
                    break;
                case "BodyColorBlue":
                    Debug.Log("Action for Two - Item " + item);
                    break;
            }
        }

        /// <summary>
        /// Select this menu option and display its sub-items.
        /// </summary>
        public void Select()
        {
            Debug.Log("Selecting " + Name);
            IsSelected = true;

            ButtonElement.AddToClassList("selected");
            ButtonElement.RemoveFromClassList("unselected");
            
            // Clear previous items and display this option's sub-items
            ReDrawItems();
        }
        public void ReDrawItems()
        {
            var itemContainer = root.Q<VisualElement>("MenuItemsContainer");
            // Clear previous items and display this option's sub-items
            itemContainer.Clear();
            foreach (var item in Items)
            {
                itemContainer.Add(item.Element);
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
        public VisualElement Element;     // UI Button for this sub-item
        public bool IsSelected;          // Whether this sub-item is selected

        public MenuItem()
        {
            IsSelected = false;
        }
        public MenuItem(string name, VisualElement element)
        {
            ID = name;
            Element = element;
            IsSelected = false;
        }

        public void Select()
        {
            IsSelected = true;
            Element.AddToClassList("item-selected");
        }

        public void Deselect()
        {
            IsSelected = false;
            Element.RemoveFromClassList("item-selected");
        }
    }
}
