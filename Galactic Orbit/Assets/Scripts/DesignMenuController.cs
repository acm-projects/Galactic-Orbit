using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// Controls the main design menu UI: 
/// - Collects materials from the character
/// - Creates and manages menu options + sub-items
/// - Handles color editing and selection logic
/// - Saves customization to Firebase on exit
/// </summary>
public class DesignMenuController : MonoBehaviour
{
    [Header("Character Reference")]
    public GameObject Character;

    [Header("UI Buttons")]
    public Button exitButton;

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
    private Dictionary<string, List<string>> menuItems;

    // --- Loading ---
    private bool MenuItemsLoaded;
    private bool CustomizationLoaded;

    // --- Unity Lifecycle ---
    private void Start()
    {
        ValidateMaterials();
    }

    private void OnEnable()
    {
        MenuItemsLoaded = false;
        CustomizationLoaded = false;
        var root = GetComponent<UIDocument>().rootVisualElement;
        menuButtons = new List<MenuOption>();

        CollectCharacterMaterials();
        StartCoroutine(InitializeMenuItems());
        
        // Load saved customization
        StartCoroutine(LoadCustomization());
        
        StartCoroutine(WaitToCreateMenuOptions(root));
        
        // Setup exit button
        SetupButtons(root);
    }
    private IEnumerator WaitToCreateMenuOptions(VisualElement root)
    {
        while (!MenuItemsLoaded || !CustomizationLoaded)
            yield return null;
        CreateMenuOptions(root);
        
    }

    private void OnDisable()
    {
        if (exitButton != null)
            exitButton.clicked -= OnExitClicked;
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

    private IEnumerator InitializeMenuItems()
    {
        menuItems = new Dictionary<string, List<string>>
        {
            { "Skin Color",      new List<string> { "rgb" } },
            { "Primary Color",   new List<string> { "rgb" } },
            { "Secondary Color", new List<string> { "rgb" } },
            { "Tertiary Color",  new List<string> { "rgb" } },
            { "Accent Color 1",  new List<string> { "rgb" } },
            { "Accent Color 2",  new List<string> { "rgb" } },

            { "Eyes", new List<string>
                { "Eyes1", "Eyes2", "Eyes3", "Eyes4" }
            },

            { "Mouth", new List<string>
                { "Mouth1", "Mouth2", "Mouth3", "Mouth4" }
            },

            { "Face Decoration", new List<string>
                { "Decor1", "Decor2", "Decor3", "Decor4" }
            }
        };

        bool eyesLoaded = false;
        UserProfileManager.Instance.HasItem("Eyes5", (success) =>
        {
            if (success) {menuItems["Eyes"].Add("Eyes5");}
            eyesLoaded = true;
        });

        bool mouthLoaded = false;
        UserProfileManager.Instance.HasItem("Mouth5", (success) =>
        {
            if (success) {menuItems["Mouth"].Add("Mouth5");}
             mouthLoaded = true;
        });

        bool decorLoaded = false;
        UserProfileManager.Instance.HasItem("Decor5", (success) =>
        {
            if (success) {menuItems["Face Decoration"].Add("Decor5");}
            decorLoaded = true;
        });
        
        while (!eyesLoaded || !mouthLoaded || !decorLoaded)
            yield return null;
        MenuItemsLoaded = true;
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

    private void SetupButtons(VisualElement root)
    {
        exitButton = root.Q<Button>("BackButton");

        if (exitButton != null)
            exitButton.clicked += OnExitClicked;
    }

    #endregion

    // =======================================================================
    #region --- Firebase Save/Load ---
    // =======================================================================

    private IEnumerator LoadCustomization()
    {
        bool cusomizationLoaded = false;
        UserProfileManager.Instance.LoadCharacterCustomization((customization) =>
        {
            // Apply to materials
            ApplyCustomization(customization);
            cusomizationLoaded = true;
        });
        while (!cusomizationLoaded)
            yield return null;
        CustomizationLoaded = true;
    }

    private void ApplyCustomization(CharacterCustomization customization)
    {
        // Apply colors
        foreach (var mat in PrimaryMaterial) mat.color = customization.primaryColor;
        foreach (var mat in SecondaryMaterial) mat.color = customization.secondaryColor;
        foreach (var mat in TertiaryMaterial) mat.color = customization.tertiaryColor;
        foreach (var mat in AccentMaterial_01) mat.color = customization.accent1Color;
        foreach (var mat in AccentMaterial_02) mat.color = customization.accent2Color;
        foreach (var mat in SkinColor) mat.color = customization.skinColor;
        
        // Apply face textures
        if (Face != null)
        {
            Texture2D[] faceTextures = Resources.LoadAll<Texture2D>("Faces");
            
            Texture2D eyesTex = Array.Find(faceTextures, t => t.name == customization.selectedEyes);
            if (eyesTex != null) Face.SetTexture("_Eyes", eyesTex);
            
            Texture2D mouthTex = Array.Find(faceTextures, t => t.name == customization.selectedMouth);
            if (mouthTex != null) Face.SetTexture("_Mouth", mouthTex);
            
            Texture2D decorTex = Array.Find(faceTextures, t => t.name == customization.selectedFaceDecoration);
            if (decorTex != null) Face.SetTexture("_Decoration", decorTex);
        }
    }

    private CharacterCustomization GetCurrentCustomization()
    {
        var customization = new CharacterCustomization();
        
        // Get colors from materials
        if (PrimaryMaterial.Count > 0) customization.primaryColor = PrimaryMaterial[0].color;
        if (SecondaryMaterial.Count > 0) customization.secondaryColor = SecondaryMaterial[0].color;
        if (TertiaryMaterial.Count > 0) customization.tertiaryColor = TertiaryMaterial[0].color;
        if (AccentMaterial_01.Count > 0) customization.accent1Color = AccentMaterial_01[0].color;
        if (AccentMaterial_02.Count > 0) customization.accent2Color = AccentMaterial_02[0].color;
        if (SkinColor.Count > 0) customization.skinColor = SkinColor[0].color;
        
        // Get selected face options from menu
        foreach (var option in menuButtons)
        {
            if (option.Name == "Eyes" && option.SelectedItem != null)
                customization.selectedEyes = option.SelectedItem.ID;
            else if (option.Name == "Mouth" && option.SelectedItem != null)
                customization.selectedMouth = option.SelectedItem.ID;
            else if (option.Name == "Face Decoration" && option.SelectedItem != null)
                customization.selectedFaceDecoration = option.SelectedItem.ID;
        }
        
        return customization;
    }

    private void OnExitClicked()
    {
        // Get current customization and save to Firebase
        CharacterCustomization currentCustomization = GetCurrentCustomization();
        
        UserProfileManager.Instance.SaveCharacterCustomization(currentCustomization, (success, message) =>
        {
            if (success)
            {
                Debug.Log("Character customization saved!");
            }
            else
            {
                Debug.LogError("Failed to save: " + message);
            }
            
            // Close menu regardless of save success/failure
            //gameObject.SetActive(false);
        });
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

        public MenuOption(string name, Button buttonElement, List<string> items, VisualElement rootNode, List<Material> materials = null)
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

                // get texture
                Texture tex = null;
                if (name.Contains("Eyes")) tex = Materials[0].GetTexture("_Eyes");
                else if (name.Contains("Mouth")) tex = Materials[0].GetTexture("_Mouth");
                else if (name.Contains("Decor")) tex = Materials[0].GetTexture("_Decoration");

                string textureName = "";
                if (tex != null) textureName = tex.name;

                // Auto-select first sub-item
                if (SelectedItem == null && textureName == item)
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