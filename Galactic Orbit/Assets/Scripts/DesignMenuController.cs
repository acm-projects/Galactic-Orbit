using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// Controls the main design menu UI: 
/// - Collects materials from the character
/// - Creates and manages menu options + sub-items
/// - Handles color editing and selection logic
/// </summary>
public class DesignMenuController : MonoBehaviour
{
    [Header("Character Reference")]
    public GameObject Character;

    // --- Material Groups ---
    private readonly List<Material> PrimaryMaterial = new();
    private readonly List<Material> SecondaryMaterial = new();
    private readonly List<Material> TertiaryMaterial = new();
    private readonly List<Material> AccentMaterial_01 = new();
    private readonly List<Material> AccentMaterial_02 = new();
    private readonly List<Material> SkinColor = new();
    private Material Face;

    // --- Menu Data ---
    private List<MenuOption> menuButtons;
    private MenuOption selectedOption;
    private Dictionary<string, string[]> menuItems;

    // --- Unity Lifecycle ---
    private void Start()
    {
        ValidateMaterials();
    }

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        menuButtons = new List<MenuOption>();

        CollectCharacterMaterials();
        InitializeMenuItems();
        CreateMenuOptions(root);
    }

    // =======================================================================
    #region --- Initialization Helpers ---
    // =======================================================================

    private void ValidateMaterials()
    {
        if (PrimaryMaterial == null ||
            SecondaryMaterial == null ||
            TertiaryMaterial == null ||
            AccentMaterial_01 == null ||
            AccentMaterial_02 == null)
        {
            Debug.LogWarning("Some material lists are not initialized.");
        }
    }

    private void CollectCharacterMaterials()
    {
        if (Character == null)
        {
            Debug.LogError("Character GameObject is not assigned!");
            return;
        }

        foreach (var renderer in Character.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.name.StartsWith("Primary")) PrimaryMaterial.Add(mat);
                else if (mat.name.StartsWith("Secondary")) SecondaryMaterial.Add(mat);
                else if (mat.name.StartsWith("Tertiary")) TertiaryMaterial.Add(mat);
                else if (mat.name.StartsWith("Accent1")) AccentMaterial_01.Add(mat);
                else if (mat.name.StartsWith("Accent2")) AccentMaterial_02.Add(mat);
                else if (mat.name.StartsWith("SkinColor")) SkinColor.Add(mat);
                else if (mat.name.Contains("Face")) Face = mat;
            }
        }
    }

    private void InitializeMenuItems()
    {
        menuItems = new Dictionary<string, string[]>
        {
            { "Skin Color",     new[] { "rgb" } },
            { "Primary Color",  new[] { "rgb" } },
            { "Secondary Color",new[] { "rgb" } },
            { "Tertiary Color", new[] { "rgb" } },
            { "Accent Color 1", new[] { "rgb" } },
            { "Accent Color 2", new[] { "rgb" } },
            { "Eyes",           new[] { "Eyes1", "Eyes2", "Eyes3", "Eyes4" } },
            { "Mouth",          new[] { "Mouth1", "Mouth2", "Mouth3", "Mouth4" } },
            { "Face Decoration",new[] { "Decor1", "Decor2", "Decor3", "Decor4" } },
        };
    }

    private void CreateMenuOptions(VisualElement root)
    {
        foreach (var item in menuItems)
        {
            // Create button
            var button = new Button { text = item.Key };

            button.AddToClassList("menu-button");
            button.AddToClassList("unselected");

            // Determine material list for this menu option
            var matList = GetMaterialListForOption(item.Key);

            // Create MenuOption
            var option = new MenuOption(item.Key, button, item.Value, root, matList);

            // Select first option by default
            if (selectedOption == null)
            {
                option.Select();
                selectedOption = option;
            }

            menuButtons.Add(option);

            // Hook click
            button.clicked += () => OnMenuButtonClicked(option);
        }
    }

    private List<Material> GetMaterialListForOption(string optionName)
    {
        if (optionName.Contains("Primary"))       return PrimaryMaterial;
        if (optionName.Contains("Secondary"))     return SecondaryMaterial;
        if (optionName.Contains("Tertiary"))      return TertiaryMaterial;
        if (optionName.Contains("Accent Color 1"))return AccentMaterial_01;
        if (optionName.Contains("Accent Color 2"))return AccentMaterial_02;
        if (optionName.Contains("Skin Color")) return SkinColor;
        if (optionName.Contains("Eyes")) return new List<Material>{Face};
        if (optionName.Contains("Mouth")) return new List<Material>{Face};
        if (optionName.Contains("Decor")) return new List<Material>{Face};
        return null;
    }

    #endregion

    // =======================================================================
    #region --- Event Handlers ---
    // =======================================================================

    private void OnMenuButtonClicked(MenuOption option)
    {
        selectedOption?.Deselect();
        option.Select();
        selectedOption = option;
    }

    #endregion

    // =======================================================================
    #region --- Nested Classes ---
    // =======================================================================

    /// <summary>
    /// Represents a top-level menu option (button + sub-items).
    /// </summary>
    private class MenuOption
    {
        public string Name;
        public Button ButtonElement;
        public List<MenuItem> Items;
        public bool IsSelected;
        public MenuItem SelectedItem;
        public VisualElement root;
        public List<Material> Materials;
        public Texture2D[] BackgroundTextures;

        public MenuOption(string name, Button buttonElement, string[] items, VisualElement rootNode, List<Material> materials = null)
        {
            BackgroundTextures = Resources.LoadAll<Texture2D>("Faces"); 
            Name = name;
            ButtonElement = buttonElement;
            Items = new List<MenuItem>();
            root = rootNode;
            Materials = materials;

            // Add button to UI
            root.Q<VisualElement>("MenuTitleContainer").Add(buttonElement);

            // Create sub-items
            foreach (var item in items)
            {
                var menuItem = CreateMenuItem(item);
                Items.Add(menuItem);

                // Auto-select first sub-item
                if (SelectedItem == null)
                {
                    SelectedItem = menuItem;
                    menuItem.Select();
                }
            }
        }

        private MenuItem CreateMenuItem(string item)
        {
            if (item.StartsWith("rgb") && Materials != null && Materials.Count > 0)
            {
                var selector = new RGBColorSelector(Materials[0].color);
                selector.AddToClassList("rgb-color-selector");

                RegisterRGBEvents(selector);
                return new MenuItem(item, selector);
            }
            else
            {
                var subButton = new Button { text = item };
                subButton.AddToClassList("menu-item");

                
                foreach (var texture in BackgroundTextures)
                {
                
                    if (texture.name.Equals(item))
                    {
                        Sprite backgroundSprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new UnityEngine.Vector2(0.5f, 0.5f), // pivot in the center
                            100f                     // pixels per unit (doesn't really matter for UI)
                        );
                        subButton.text = "";
                        
                        subButton.style.backgroundImage = new StyleBackground(backgroundSprite);
                        subButton.style.backgroundSize = new BackgroundSize(
                            new Length(150, LengthUnit.Percent),
                            new Length(150, LengthUnit.Percent)
                        );

                        subButton.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        subButton.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        subButton.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    }
                }
            

                var menuItem = new MenuItem(item, subButton);
                subButton.clicked += () =>
                {
                    DoMenuEvent(item, Materials[0]);
                    SelectedItem?.Deselect();
                    SelectedItem = menuItem;
                    menuItem.Select();
                };

                return menuItem;
            }
        }

        private void RegisterRGBEvents(RGBColorSelector selector)
        {
            foreach (var mat in Materials)
            {
                selector.rSlider.RegisterValueChangedCallback(evt =>
                    mat.color = new Color(selector.rSlider.value, selector.gSlider.value, selector.bSlider.value));

                selector.gSlider.RegisterValueChangedCallback(evt =>
                    mat.color = new Color(selector.rSlider.value, selector.gSlider.value, selector.bSlider.value));

                selector.bSlider.RegisterValueChangedCallback(evt =>
                    mat.color = new Color(selector.rSlider.value, selector.gSlider.value, selector.bSlider.value));
            }
        }

        private void DoMenuEvent(string item, Material mat)
        {
            Texture2D tex = Array.Find(BackgroundTextures, t => t.name == item);
            
            if (item.Contains("Eyes"))
            {
                mat.SetTexture("_Eyes", tex);
            }
            else if (item.Contains("Mouth"))
            {
                mat.SetTexture("_Mouth", tex);
            }
            else if (item.Contains("Decor"))
            {
                mat.SetTexture("_Decoration", tex);
            }
        }

        public void Select()
        {
            IsSelected = true;
            ButtonElement.AddToClassList("selected");
            ButtonElement.RemoveFromClassList("unselected");
            RedrawItems();
        }

        public void Deselect()
        {
            IsSelected = false;
            ButtonElement.RemoveFromClassList("selected");
            ButtonElement.AddToClassList("unselected");
        }

        private void RedrawItems()
        {
            var container = root.Q<VisualElement>("MenuItemsContainer");
            container.Clear();
            foreach (var item in Items)
            {
                container.Add(item.Element);
            }
        }
    }

    /// <summary>
    /// Represents a sub-item inside a menu option (button or RGB selector).
    /// </summary>
    private class MenuItem
    {
        public string ID;
        public Image Preview;
        public VisualElement Element;
        public bool IsSelected;

        public MenuItem(string id, VisualElement element, Image Preview=null)
        {
            ID = id;
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

    #endregion
}
