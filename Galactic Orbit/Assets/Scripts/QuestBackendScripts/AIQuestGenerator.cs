using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;

public class AIQuestGenerator
{
    // Ollama API endpoint (runs locally)
    private const string OLLAMA_API = "http://localhost:11434/api/generate";
    private const string MODEL = "llama3.2";

    // UTD-specific locations for quest generation
    private static readonly string[] UTD_LOCATIONS = {
        "Eugene McDermott Library",
        "Student Union",
        "Activity Center",
        "Residence Hall West",
        "Jonsson Academic Center", 
        "Engineering and Computer Science Complex",
        "Natural Science and Engineering Research Laboratory",
        "Callier Center",
        "Founders Building",
        "Green Center",
        "Student Services Building"
    };

    // Generate a UTD-specific quest using Llama
    public static async Task<Quest> GenerateUTDQuest()
    {
        // Pick a random UTD location
        string location = UTD_LOCATIONS[Random.Range(0, UTD_LOCATIONS.Length)];

        string prompt = $@"Create a campus AR quest for University of Texas at Dallas students.

Location: {location}

Generate:
1. A catchy quest title (max 8 words)
2. A fun 2-sentence description that tells students what to do
3. Reward points (between 50-200)

Format your response EXACTLY like this:
TITLE: [your title here]
DESCRIPTION: [your description here]
POINTS: [number only]

Make it engaging and related to {location}!";

        try
        {
            Debug.Log("🤖 Generating quest with Llama...");
            
            // Call Ollama API
            string response = await CallOllamaAPI(prompt);
            
            Debug.Log($"📝 AI Response: {response}");
            
            // Parse the response
            Quest quest = ParseQuestFromResponse(response, location);
            
            if (quest != null)
            {
                Debug.Log($"✅ Generated quest: {quest.questTitle}");
                return quest;
            }
            else
            {
                Debug.LogWarning("Failed to parse AI response, using fallback...");
                return GenerateFallbackQuest(location);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ AI Generation failed: {e.Message}");
            return GenerateFallbackQuest(location);
        }
    }

    // Call Ollama's local API
    private static async Task<string> CallOllamaAPI(string prompt)
    {
        var requestBody = new
        {
            model = MODEL,
            prompt = prompt,
            stream = false
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(OLLAMA_API, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            // Wait for completion
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                OllamaResponse response = JsonUtility.FromJson<OllamaResponse>(jsonResponse);
                return response.response;
            }
            else
            {
                throw new System.Exception($"API Error: {request.error}");
            }
        }
    }

    // Parse AI response into Quest object
    private static Quest ParseQuestFromResponse(string response, string location)
    {
        try
        {
            string title = "";
            string description = "";
            int points = 100;

            // Parse TITLE
            int titleStart = response.IndexOf("TITLE:") + 6;
            int titleEnd = response.IndexOf("DESCRIPTION:");
            if (titleStart > 5 && titleEnd > titleStart)
            {
                title = response.Substring(titleStart, titleEnd - titleStart).Trim();
            }

            // Parse DESCRIPTION
            int descStart = response.IndexOf("DESCRIPTION:") + 12;
            int descEnd = response.IndexOf("POINTS:");
            if (descStart > 11 && descEnd > descStart)
            {
                description = response.Substring(descStart, descEnd - descStart).Trim();
            }

            // Parse POINTS
            int pointsStart = response.IndexOf("POINTS:") + 7;
            if (pointsStart > 6)
            {
                string pointsStr = response.Substring(pointsStart).Trim();
                // Extract just the number
                pointsStr = new string(System.Array.FindAll(pointsStr.ToCharArray(), char.IsDigit));
                if (int.TryParse(pointsStr, out int parsedPoints))
                {
                    points = Mathf.Clamp(parsedPoints, 50, 200);
                }
            }

            // Validate we got data
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                return null;
            }

            // Create quest with UTD coordinates (using Student Union as default)
            Vector2 utdLocation = new Vector2(32.9857f, -96.7501f);

            return QuestManager.Instance.AddRuntimeQuest(
                $"AI_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                title,
                description,
                utdLocation,
                points
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Parse error: {e.Message}");
            return null;
        }
    }

    // Fallback if AI fails
    private static Quest GenerateFallbackQuest(string location)
    {
        string[] templates = {
            $"Discover the secrets of {location}",
            $"Find the hidden marker at {location}",
            $"Explore {location} and scan the AR object"
        };

        string title = templates[Random.Range(0, templates.Length)];
        string description = $"Visit {location} and complete the challenge to earn points!";
        Vector2 utdLocation = new Vector2(32.9857f, -96.7501f);

        return QuestManager.Instance.AddRuntimeQuest(
            $"FALL_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
            title,
            description,
            utdLocation,
            Random.Range(50, 200)
        );
    }

    // Helper class for JSON parsing
    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }
}