using System.Collections;
using TMPro;
using UnityEngine;


public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float charsPerSecond = 40f;

    private TextMeshProUGUI label;
    private Coroutine typingCo;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

        public void Play(string text, bool addNewline = true)
    {
        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = StartCoroutine(Type(text, addNewline));
    }

    private IEnumerator Type(string text, bool addNewline)
    {
        if (label == null) yield break;

        // Preserve existing text and append.
        if (addNewline && !string.IsNullOrEmpty(label.text))
            label.text += "\n";

        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        foreach (char c in text)
        {
            label.text += c;
            yield return new WaitForSeconds(delay);
        }

        typingCo = null;
    }
}
