using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Pool (Assign ScriptableObjects here)")]
    public List<Quest> allQuests = new List<Quest>();
    
    [Header("Active Quests (Runtime)")]
    public List<Quest> activeQuests = new List<Quest>();
    
    [Header("Selected Quest (for UI)")]
    public Quest selectedQuest { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Optional: Auto-add some test quests at start
        // You can remove this once you have ScriptableObject quests
        AddRuntimeQuest("Q001", "Visit the Library", "Go to the main library and scan the AR marker.", new Vector2(40.7128f, -74.0060f), 100);
        AddRuntimeQuest("Q002", "Attend an Event", "Check out the student fair near the main hall.", new Vector2(40.7135f, -74.0055f), 150);
    }

    // === QUEST POOL MANAGEMENT (for UI) ===
    
    // Get random quests from the pool (used by UI)
    public List<Quest> GetUniqueRandomQuests(int count)
    {
        if (count >= allQuests.Count)
        {
            return new List<Quest>(allQuests);
        }

        List<Quest> availableQuests = new List<Quest>(allQuests);
        List<Quest> chosenQuests = new List<Quest>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableQuests.Count);
            chosenQuests.Add(availableQuests[randomIndex]);
            availableQuests.RemoveAt(randomIndex);
        }

        return chosenQuests;
    }

    // === ACTIVE QUEST MANAGEMENT ===
    
    // Activate a quest (player accepts it)
    public void ActivateQuest(Quest quest)
    {
        if (quest == null) return;
        
        if (!activeQuests.Contains(quest))
        {
            quest.isActive = true;
            quest.isCompleted = false;
            activeQuests.Add(quest);
            Debug.Log($"✅ Quest activated: {quest.questTitle}");
        }
        else
        {
            Debug.LogWarning($"Quest '{quest.questTitle}' is already active!");
        }
    }

    // Select a quest (for UI details view)
    public void SelectQuest(Quest quest)
    {
        selectedQuest = quest;
        Debug.Log($"Quest selected: {quest.questTitle}");
    }

    // Complete a quest (called by ARLocationManager or other systems)
    public void CompleteQuest(string questID)
    {
        Quest quest = activeQuests.Find(q => q.questID == questID);
        
        if (quest != null && !quest.isCompleted)
        {
            quest.isCompleted = true;
            quest.isActive = false;
            
            Debug.Log($"🎉 Quest completed: {quest.questTitle}! Awarding {quest.rewardPoints} points...");
            
            // Award points via UserProfileManager
            if (UserProfileManager.Instance != null)
            {
                UserProfileManager.Instance.AddPoints(quest.rewardPoints, (success, msg) => 
                {
                    if (success)
                    {
                        Debug.Log($"✅ {msg}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to award points: {msg}");
                    }
                });
            }
            else
            {
                Debug.LogWarning("UserProfileManager not found! Points not awarded.");
            }
        }
        else if (quest == null)
        {
            Debug.LogWarning($"Quest with ID '{questID}' not found in active quests.");
        }
        else
        {
            Debug.LogWarning($"Quest '{quest.questTitle}' is already completed!");
        }
    }

    // === RUNTIME QUEST CREATION (for AI-generated quests) ===
    
    // Add a quest created at runtime (for AI generation)
    public Quest AddRuntimeQuest(string id, string title, string desc, Vector2 location, int reward)
    {
        Quest newQuest = Quest.CreateRuntimeQuest(id, title, desc, location, reward);
        allQuests.Add(newQuest);
        Debug.Log($"📝 Runtime quest added: {title}");
        return newQuest;
    }

    // === UTILITY ===
    
    // Get all completed quests
    public List<Quest> GetCompletedQuests()
    {
        return activeQuests.FindAll(q => q.isCompleted);
    }

    // Get all incomplete active quests
    public List<Quest> GetIncompleteQuests()
    {
        return activeQuests.FindAll(q => !q.isCompleted && q.isActive);
    }
}