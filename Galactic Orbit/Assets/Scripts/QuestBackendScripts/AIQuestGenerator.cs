using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;

public class AIQuestGenerator
{
    // llama.cpp server endpoint
    private const string LLAMA_API = "http://127.0.0.1:8080/completion";
    
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

    private static readonly string[] UTD_LOCATIONS_FROM_CSV = {
        "Eugene McDermott Library (MC)",
        "Student Union (SU)",
        "Activity Center (AB)",
        "University Theatre (TH)",
        "Erik Jonsson Academic Center (JO)", 
        "Engineering and Computer Science South (ECSS)",
        "Engineering and Computer Science West (ESCW)",
        "Engineering and Computer Science North (ECSN)",
        "Cecil H. Green Hall (GR)",
        "Bioengineering and Sciences Building (BSB)",
        "Administration Building (AD)",
        "Sciences Building (SCI)",
        "Callier Center Richardson (CR)",
        "Founders Building (FO)",
        "Founders North (FN)",
        "Classroom Building (CB)",
        "Karl Hoblitzelle Hall (HH)",
        "Student Services Building (SSB)",
        "Student Services Building Addition (SSA)",
        "Naveen Jindal School of Management (JSOM)",
        "Science Learning Center (SLC)"
    };

    private static Dictionary<string, (double lat, double lon)> lookup;

    public static void LoadCsv()
    {
        lookup = new Dictionary<string, (double lat, double lon)>();

        TextAsset csv = Resources.Load<TextAsset>("BuildingLocations");

        if (csv == null)
        {
            Debug.LogError("Could not find BuildingLocations.csv in Resources/");
            return;
        }

        string[] lines = csv.text.Split('\n');

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            string building = parts[0].Trim();
            double lat = double.Parse(parts[1]);
            double lon = double.Parse(parts[2]);

            lookup[building] = (lat, lon);
        }
    }

    public static (double lat, double lon)? GetCoordinates(string buildingName)
    {
        if (lookup == null)
            LoadCsv();

        if (lookup.TryGetValue(buildingName, out var coords))
            return coords;

        return null;
    }


    // Generate a UTD-specific quest using Llama
    public static async Task<Quest> GenerateUTDQuest()
    {
        // Pick a random UTD location
        //string location = UTD_LOCATIONS[Random.Range(0, UTD_LOCATIONS.Length)];
        string location = UTD_LOCATIONS_FROM_CSV[Random.Range(0, UTD_LOCATIONS.Length)];

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
            
            // Call llama.cpp API
            string response = await CallLlamaCppAPI(prompt);
            
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

    // Call llama.cpp API
    private static async Task<string> CallLlamaCppAPI(string prompt)
    {
        // llama.cpp completion endpoint format
        var requestData = new LlamaCppRequest
        {
            prompt = prompt,
            n_predict = 200,  // Max tokens to generate
            temperature = 0.7f,
            top_k = 40,
            top_p = 0.9f,
            stop = new string[] { "\n\n", "USER:", "ASSISTANT:" }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(LLAMA_API, "POST"))
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
                Debug.Log($"Raw response: {jsonResponse}");
                
                LlamaCppResponse response = JsonUtility.FromJson<LlamaCppResponse>(jsonResponse);
                return response.content;
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

            // Create quest with UTD coordinatesVector2 realCoordinates = UTDLocationDatabase.GetCoordinates(location);
            Vector2 realCoordinates = UTDLocationDatabase.GetCoordinates(location);

            return QuestManager.Instance.AddRuntimeQuest(
                $"AI_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                title,
                description,
                realCoordinates,
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
        //Vector2 realCoordinates = UTDLocationDatabase.GetCoordinates(location);
        var coords = GetCoordinates(location);

        if (coords != null)
        {
            Debug.Log($"Lat: {coords.Value.lat}, Lon: {coords.Value.lon}");
        }
        else
        {
            Debug.Log("Building not found.");
            return null;
        }


        return QuestManager.Instance.AddRuntimeQuest(
            $"FALL_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
            title,
            description,
            new Vector2((float)coords.Value.lat, (float)coords.Value.lon),
            Random.Range(50, 200)
        );
    }

    // Helper classes for JSON serialization
    [System.Serializable]
    private class LlamaCppRequest
    {
        public string prompt;
        public int n_predict;
        public float temperature;
        public int top_k;
        public float top_p;
        public string[] stop;
    }

    [System.Serializable]
    private class LlamaCppResponse
    {
        public string content;
    }
}