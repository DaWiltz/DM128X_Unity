using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LLMAgent : MonoBehaviour
{
    [Header("Dialogue Output")]
    [SerializeField] private DialogueAutoScrollFixed dialogueUI;

    [Header("Display")]
    [SerializeField] private string thinkingLine = "LLM: …thinking";
    [SerializeField] private string replyPrefix = "LLM: ";

    [Header("Gemini Settings")]
    [Tooltip("Google AI Studio / Gemini API key")] 
    [SerializeField] private string apiKey = "YOUR_KEY";

    [Tooltip("Model name in the Gemini API path (e.g., gemini-2.5-flash or gemini-2.5-pro)")]
    [SerializeField] private string modelName = "gemini-2.5-flash";

    [Header("UI Components")]
    [SerializeField] private TypewriterEffect typewriter; // assign DialogueText (TMP + TypewriterEffect)

    void Awake()
    {
        if (typewriter == null && dialogueUI != null)
            typewriter = dialogueUI.GetComponentInChildren<TypewriterEffect>(true);
    }

    private Coroutine activeCoroutine;

    // --- Request/Response DTOs for JsonUtility ---
    [System.Serializable] private class Part { public string text; }
    [System.Serializable] private class Content { public Part[] parts; }
    [System.Serializable] private class RequestBody { public Content[] contents; }
    [System.Serializable] private class Candidate { public Content content; }
    [System.Serializable] private class ResponseBody { public Candidate[] candidates; }

    /// <summary>
    /// Call this when the player submits input.
    /// Player text is already written elsewhere.
    /// </summary>
    public void OnPlayerInput(string playerText)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(SendToGemini(playerText));
    }

    private IEnumerator SendToGemini(string playerText)
    {
        // Build JSON body expected by Gemini
        var body = new RequestBody
        {
            contents = new[]
            {
                new Content
                {
                    parts = new[] { new Part { text = playerText } }
                }
            }
        };

        string json = JsonUtility.ToJson(body);
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ResponseBody>(uwr.downloadHandler.text);
                string replyText = replyPrefix + ExtractFirstReply(response);

                if (typewriter != null)
                    typewriter.Play(replyText);
                else
                    dialogueUI.AppendLine(replyText);
            }
            else
            {
                string error = $"Error {uwr.responseCode}: {uwr.error}";
                if (typewriter != null)
                    typewriter.Play(replyPrefix + error);
                else
                    dialogueUI.AppendLine(replyPrefix + error);
                Debug.LogError($"LLMAgent Gemini call failed: {error}\n{uwr.downloadHandler.text}");
            }
        }

        activeCoroutine = null;
    }

    /// <summary>Safely extracts the first text reply from the Gemini response.</summary>
    private static string ExtractFirstReply(ResponseBody response)
    {
        if (response?.candidates == null || response.candidates.Length == 0)
            return "(empty reply)";

        var content = response.candidates[0].content;
        if (content?.parts == null || content.parts.Length == 0)
            return "(empty reply)";

        return string.IsNullOrEmpty(content.parts[0].text) ? "(empty reply)" : content.parts[0].text;
    }
}
