using System.Collections;
using UnityEngine;

public class LLMAgent : MonoBehaviour
{
    [Header("Dialogue Output")]
    [SerializeField] private DialogueAutoScrollFixed dialogueUI;

    [Header("Display")]
    [SerializeField] private string thinkingLine = "LLM: …thinking";
    [SerializeField] private string replyPrefix = "LLM: ";

    [Header("Timing")]
    [SerializeField] private float fakeDelaySeconds = 1.0f;

    private Coroutine activeCoroutine;

    /// <summary>
    /// Call this when the player submits input.
    /// Player text is already written elsewhere.
    /// </summary>
    public void OnPlayerInput(string playerText)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(FakeLLMResponse(playerText));
    }

    private IEnumerator FakeLLMResponse(string playerText)
    {
        // Show placeholder
        dialogueUI.AppendLine(thinkingLine);

        // Simulate API delay
        yield return new WaitForSeconds(fakeDelaySeconds);

        // Append placeholder response that includes player input
        dialogueUI.AppendLine(
            replyPrefix + $"You said: \"{playerText}\". I am processing that."
        );

        activeCoroutine = null;
    }
}
