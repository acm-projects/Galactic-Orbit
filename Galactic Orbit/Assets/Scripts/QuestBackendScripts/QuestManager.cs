using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine;
using System;
using NUnit.Framework;
using System.Linq;
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Pool (Assign ScriptableObjects here)")]
    public List<Quest> allQuests = new List<Quest>();
    
    [Header("Active Quests (Runtime)")]
    public List<Quest> activeQuests = new List<Quest>();
    
    [Header("Selected Quest (for UI)")]
    public Quest selectedQuest { get; private set; }

    [Header("Auto-Generation Settings")]
    public bool generateAIQuestsOnStart = true;
    public int numberOfAIQuests = 5;

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

    // Quest Implementation
    public List<Quest> GetNearbyQuests(Vector2d coordinates)
    {
        List<Quest> nearby = new List<Quest>();
        Debug.Log("NUMBER OF ACTIVE: " + activeQuests.Count);
        foreach (var quest in activeQuests)
        {
            if (IsNearby(quest, coordinates) && !quest.isCompleted)
            {
                nearby.Add(quest);
            }
        }
        Debug.Log("NUMBER OF NEARBY: " + nearby.Count);
        return nearby;
    }
    public bool IsNearby(Quest quest, Vector2d coordinates)
    {
        double distance = HaversineDistance(
            coordinates.x, coordinates.y,
            quest.targetLocation.x, quest.targetLocation.y
        );
        Debug.Log(quest.questTitle + "- Distance from user: " + distance);
        return distance <= quest.completionRadius;
    }
    private const double EarthRadius = 6371000; // meters

    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = Deg2Rad(lat2 - lat1);
        double dLon = Deg2Rad(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c;
    }

    private double Deg2Rad(double deg)
    {
        return deg * Math.PI / 180.0;
    }


    async void Start()
    {
        // Optional: Remove test quests if you no longer want them
        // AddRuntimeQuest("Q001", "Visit the Library", "Go to the main library and scan the AR marker.", new Vector2(40.7128f, -74.0060f), 100);
        // AddRuntimeQuest("Q002", "Attend an Event", "Check out the student fair near the main hall.", new Vector2(40.7135f, -74.0055f), 150);

        if (generateAIQuestsOnStart)
        {
            Debug.Log("⚙️ Auto-generating AI quests on start...");

            for (int i = 0; i < numberOfAIQuests; i++)
            {
                Quest aiQuest = await AIQuestGenerator.GenerateUTDQuest();

                if (aiQuest != null)
                    Debug.Log($"✅ AI Quest {i + 1} generated: {aiQuest.questTitle}");
                else
                    Debug.LogError($"❌ Failed to generate AI Quest {i + 1}");

                await System.Threading.Tasks.Task.Delay(300); // Optional small delay between generations
            }

            // Refresh Quest UI after all quests are generated
            QuestUI questUI = FindFirstObjectByType<QuestUI>();
            if (questUI != null)
            {
                Debug.Log("🔄 Updating quest UI...");
                questUI.PullNewQuests();
            }
            else
            {
                Debug.LogWarning("⚠️ No QuestUI found to refresh UI.");
            }
        }
    }


    // === QUEST POOL MANAGEMENT (for UI) ===
    
    // Get random quests from the pool (used by UI)
    public List<Quest> GetUniqueRandomQuests(int count)
    {
        if (count >= allQuests.Count)
        {
            // DEBUGGING
            Quest testQuest = Quest.CreateRuntimeQuest("Explore Engineering and Computer Science West (ECSW) for it's secrets", "Explore Engineering and Computer Science West (ECSW) Building to get the AR Object", "The Engineering and Computer Science West (ECSW) Building is hiding an AR item. Scan it to get points!", new Vector2(32.98582f, -96.75130f), 56);
            
            Quest testQuest1 = Quest.CreateRuntimeQuest("Explore Engineering and Computer Science Souuth (ECSS) for it's secrets", "Find the object hidden in the Engineering and Computer Science West (ECSS) Building.", "The Engineering and Computer Science South (ECSS) Building is hiding an AR item. Scan it to get points!", new Vector2(32.98634f, -96.75004f), 78);
            allQuests[0] = testQuest;
            allQuests[1] = testQuest1;
            
            return allQuests.Take(count).ToList();
        }

        List<Quest> availableQuests = new List<Quest>(allQuests);
        List<Quest> chosenQuests = new List<Quest>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableQuests.Count);
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
    public void DeactivateQuest(Quest quest)
    {
        if (quest == null) return;
        
        if (activeQuests.Contains(quest))
        {
            quest.isActive = false;
            quest.isCompleted = true;
            Debug.Log($"✅ Quest deactivated: {quest.questTitle}");
        }
        else
        {
            Debug.LogWarning($"Quest '{quest.questTitle}' is not active!");
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
            DeactivateQuest(quest);
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

                // Add 10 coins --hardcoded
                UserProfileManager.Instance.AddCoins(10, (success, msg) => 
                {
                    if (success)
                    {
                        Debug.Log($"✅ {msg}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to add coins: {msg}");
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

        // ✅ Detect if it’s AI-generated
        bool isAI = id.StartsWith("AI_");
        string source = isAI ? "🤖 [AI Generated]" : "📘 [Manual]";

        Debug.Log($"{source} Quest added: {title}");

        return newQuest;
    }

    // ===== Proximity/Real Location checking =====

    /// <summary>
    /// Check if player is near enough to start a quest
    /// </summary>
    public bool IsQuestNearby(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);
        if (quest == null)
        {
            Debug.LogWarning($"Quest {questID} not found");
            return false;
        }

        if (ProximityChecker.Instance == null)
        {
            Debug.LogWarning("ProximityChecker not found in scene");
            return false;
        }

        return ProximityChecker.Instance.IsPlayerNearQuest(quest);
    }

    /// <summary>
    /// Get distance to a quest location in meters
    /// </summary>
    public float GetDistanceToQuest(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);
        if (quest == null) return float.MaxValue;

        if (ProximityChecker.Instance == null) return float.MaxValue;

        return ProximityChecker.Instance.GetDistanceToQuest(quest);
    }

    /// <summary>
    /// Start a quest (only works if player is nearby)
    /// </summary>
    public bool StartQuest(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);
        
        if (quest == null)
        {
            Debug.LogWarning($"Quest {questID} not found");
            return false;
        }

        // Check if already active
        if (quest.isActive)
        {
            Debug.LogWarning($"Quest '{quest.questTitle}' is already active");
            return false;
        }

        // Check if already completed
        if (quest.isCompleted)
        {
            Debug.LogWarning($"Quest '{quest.questTitle}' is already completed");
            return false;
        }

        // Check proximity
        if (!IsQuestNearby(questID))
        {
            float distance = GetDistanceToQuest(questID);
            Debug.LogWarning($"Too far from quest location. Distance: {distance:F0}m, Required: {quest.completionRadius}m");
            return false;
        }

        // All checks passed - activate quest
        ActivateQuest(quest);
        Debug.Log($"✅ Started quest: {quest.questTitle}");
        return true;
    }

    /// <summary>
    /// Check if a quest can be started (is nearby and not active/completed)
    /// </summary>
    public bool CanStartQuest(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);
        
        if (quest == null) return false;
        if (quest.isActive) return false;
        if (quest.isCompleted) return false;
        
        return IsQuestNearby(questID);
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