// TEMPLATE, REPLACE WITH AI-DRIVEN CONTENT GENERATION POWERED BY OLLAMA + LLAMA 3
// It's completely free and offline, so it can be a locally hosted mode focused on
// privacy and gameplay
public class AIQuestGenerator
{
    public static string GenerateQuestDescription(string locationName)
    {
        string[] templates = {
            $"Discover the secrets of {locationName} and scan the hidden AR marker.",
            $"Head to {locationName} and find the ancient crest to earn your reward.",
            $"Explore {locationName}, take a photo of the AR statue, and claim your badge!"
        };

        return templates[UnityEngine.Random.Range(0, templates.Length)];
    }
}
