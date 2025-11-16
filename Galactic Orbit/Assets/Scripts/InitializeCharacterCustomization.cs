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
/// - Saves customization to Firebase on exit
/// </summary>
public class CharacterCustomizationInitializer : MonoBehaviour
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

    // --- Unity Lifecycle ---
    private void Start()
    {
        ValidateMaterials();
    }

    private void OnEnable()
    {

        CollectCharacterMaterials();
        
        // Load saved customization
        LoadCustomization();
        
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


    #endregion

    // =======================================================================
    #region --- Firebase Save/Load ---
    // =======================================================================

    private void LoadCustomization()
    {
        UserProfileManager.Instance.LoadCharacterCustomization((customization) =>
        {
            // Apply to materials
            ApplyCustomization(customization);
        });
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


    #endregion

    
}