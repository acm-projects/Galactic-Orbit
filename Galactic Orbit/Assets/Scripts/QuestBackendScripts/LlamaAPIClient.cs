using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LlamaAPIClient : MonoBehaviour
{
    public static LlamaAPIClient Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator GenerateQuest(string playerContext, Action<string> onResult)
    {
        string prompt = $"You are a quest generator for an AR campus exploration game. Based on the following player context, create a short quest title and description:\n\n{playerContext}\n\nFormat:\nTitle: <title>\nDescription: <description>";

        string url = "http://localhost:8080/completion";
        string json = $"{{\"prompt\": \"{EscapeJson(prompt)}\", \"n_predict\": 150}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ LLaMA API Error: " + request.error);
                onResult?.Invoke(null);
            }
            else
            {
                string response = request.downloadHandler.text;
                onResult?.Invoke(response);
            }
        }
    }

    private string EscapeJson(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
