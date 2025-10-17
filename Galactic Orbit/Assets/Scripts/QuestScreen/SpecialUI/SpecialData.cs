using UnityEngine;

[CreateAssetMenu(fileName = "New Special", menuName = "Special System/Special")]
public class SpecialData : ScriptableObject
{
    public string specialTitle;
    [TextArea(3, 10)]
    public string specialDescription;
    public string unlockRequirement; // e.g., "Level 5+", "Limited Time"
    public int cost;
}