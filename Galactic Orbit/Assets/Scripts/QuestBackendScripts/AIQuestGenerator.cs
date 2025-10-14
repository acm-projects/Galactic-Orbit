// TEMPLATE, REPLACE WITH AI-DRIVEN CONTENT GENERATION POWERED BY OLLAMA + LLAMA 3
// It's completely free and offline, locally hosted mode
using UnityEngine;
public class AIQuestGenerator
{
    public static Quest GenerateQuest(string locationName)
    {
        // For now, use templates
        string[] templates = {
            $"Discover the secrets of {locationName}",
            $"Find the hidden marker at {locationName}",
            $"Explore {locationName} and scan the AR object"
        };

        string title = templates[UnityEngine.Random.Range(0, templates.Length)];
        string description = $"Visit {locationName} and complete the challenge to earn points!";

        // Create runtime quest
        return QuestManager.Instance.AddRuntimeQuest(
            $"AI_{System.Guid.NewGuid()}",
            title,
            description,
            new Vector2(40.7128f, -74.0060f), // placeholder location
            UnityEngine.Random.Range(50, 200)
        );
    }
}
