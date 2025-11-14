using UnityEngine;

public class CharacterColorController : MonoBehaviour
{
    void Start()
    {
        LoadAndApplyCustomization();
    }
    
    public void LoadAndApplyCustomization()
    {
        UserProfileManager.Instance.LoadCharacterCustomization((customization) =>
        {
            ApplyCustomization(customization);
        });
    }
    
    public void ApplyCustomization(CharacterCustomization customization)
    {
        // Find and apply colors to all materials on this character
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.name.StartsWith("Primary")) 
                    mat.color = customization.primaryColor;
                else if (mat.name.StartsWith("Secondary")) 
                    mat.color = customization.secondaryColor;
                else if (mat.name.StartsWith("Tertiary")) 
                    mat.color = customization.tertiaryColor;
                else if (mat.name.StartsWith("Accent1")) 
                    mat.color = customization.accent1Color;
                else if (mat.name.StartsWith("Accent2")) 
                    mat.color = customization.accent2Color;
                else if (mat.name.StartsWith("SkinColor")) 
                    mat.color = customization.skinColor;
                else if (mat.name.Contains("Face"))
                {
                    // Apply face textures
                    Texture2D[] faceTextures = Resources.LoadAll<Texture2D>("Faces");
                    
                    Texture2D eyesTex = System.Array.Find(faceTextures, t => t.name == customization.selectedEyes);
                    if (eyesTex != null) mat.SetTexture("_Eyes", eyesTex);
                    
                    Texture2D mouthTex = System.Array.Find(faceTextures, t => t.name == customization.selectedMouth);
                    if (mouthTex != null) mat.SetTexture("_Mouth", mouthTex);
                    
                    Texture2D decorTex = System.Array.Find(faceTextures, t => t.name == customization.selectedFaceDecoration);
                    if (decorTex != null) mat.SetTexture("_Decoration", decorTex);
                }
            }
        }
    }
}