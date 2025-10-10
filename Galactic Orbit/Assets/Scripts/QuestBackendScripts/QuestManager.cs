using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public List<Quest> activeQuests = new List<Quest>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Example quests - later these can be AI or database generated
        AddQuest("Q001", "Visit the Library", "Go to the main library and scan the AR marker.", new Vector2(40.7128f, -74.0060f), 100);
        AddQuest("Q002", "Attend an Event", "Check out the student fair near the main hall.", new Vector2(40.7135f, -74.0055f), 150);
    }

    public void AddQuest(string id, string title, string desc, Vector2 location, int reward)
    {
        Quest newQuest = new Quest(id, title, desc, location, reward);
        activeQuests.Add(newQuest);
        Debug.Log($"New quest added: {title}");
    }

    public void CompleteQuest(string id)
    {
        Quest quest = activeQuests.Find(q => q.questID == id);
        if (quest != null)
        {
            quest.isCompleted = true;
            Debug.Log($"Quest completed: {quest.title}! You earned {quest.rewardPoints} points.");
        }
    }
}
