using TMPro;
using UnityEngine;

public class LLMInputSender : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private LLMAgent llmAgent;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        // Listen for Enter / Submit
        inputField.onSubmit.AddListener(HandleSubmit);
    }

    private void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(HandleSubmit);
    }

    private void HandleSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Forward ONLY to the LLM agent
        llmAgent.OnPlayerInput(text);
    }
}
