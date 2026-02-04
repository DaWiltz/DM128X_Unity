using TMPro;
using UnityEngine;

public class DialogueInputSender : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private DialogueAutoScrollFixed dialogue;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<TMP_InputField>();

        input.onSubmit.AddListener(OnSubmit);
    }

    private void OnDestroy()
    {
        input.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        dialogue.AppendLine(text);

        input.text = "";
        input.ActivateInputField(); // keeps focus
    }
}
