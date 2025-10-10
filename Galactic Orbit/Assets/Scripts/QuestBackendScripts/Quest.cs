using UnityEngine;

[System.Serializable]
public class Quest
{
    public string questID;
    public string title;
    public string description;
    public Vector2 targetLocation; // latitude, longitude
    public bool isCompleted;
    public int rewardPoints;

    public Quest(string id, string title, string desc, Vector2 target, int reward)
    {
        this.questID = id;
        this.title = title;
        this.description = desc;
        this.targetLocation = target;
        this.rewardPoints = reward;
        this.isCompleted = false;
    }
}
