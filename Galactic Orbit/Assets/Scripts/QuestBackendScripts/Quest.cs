using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
[System.Serializable]
public class Quest : ScriptableObject
{
    [Header("Quest Identification")]
    public string questID;
    
    [Header("Quest Info")]
    public string questTitle;
    [TextArea(3, 10)]
    public string questDescription;
    
    [Header("Location (for AR quests)")]
    public Vector2 targetLocation; // latitude, longitude
    public float completionRadius = 30f; // not meters (some new unit)
    
    [Header("Rewards")]
    public int rewardPoints;
    
    [Header("Runtime State - Do Not Edit")]
    [System.NonSerialized] public bool isCompleted;
    [System.NonSerialized] public bool isActive;

    // Constructor for runtime quest creation (for AI-generated quests later)
    public static Quest CreateRuntimeQuest(string id, string title, string desc, Vector2 location, int reward)
    {
        Quest newQuest = ScriptableObject.CreateInstance<Quest>();
        newQuest.questID = id;
        newQuest.questTitle = title;
        newQuest.questDescription = desc;
        newQuest.targetLocation = location;
        newQuest.rewardPoints = reward;
        newQuest.completionRadius = 30f;
        newQuest.isCompleted = false;
        newQuest.isActive = false;
        return newQuest;
    }
}