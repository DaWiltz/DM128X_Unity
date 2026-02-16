using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueAutoScrollFixed : MonoBehaviour
{
    [Header("Optional (auto-filled if left empty)")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Behavior")]
    [Tooltip("If true, only auto-scroll when the user is already near the bottom.")]
    [SerializeField] private bool stickToBottomOnly = true;

    [Tooltip("How close to bottom counts as 'at bottom' (0..1). 0.02 is usually fine.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float bottomThreshold = 0.02f;

    private Coroutine _scrollCo;

    private void Reset()
    {
        AutoWire();
    }

    private void Awake()
    {
        AutoWire();

        // Hard fails that explain exactly what's missing.
        Debug.Assert(scrollRect != null, "DialogueAutoScrollFixed: Missing ScrollRect.");
        Debug.Assert(viewport != null, "DialogueAutoScrollFixed: Missing Viewport RectTransform.");
        Debug.Assert(content != null, "DialogueAutoScrollFixed: Missing Content RectTransform.");
        Debug.Assert(dialogueText != null, "DialogueAutoScrollFixed: Missing TextMeshProUGUI (Dialogue Text).");
    }

    private void AutoWire()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            if (viewport == null) viewport = scrollRect.viewport;
            if (content == null) content = scrollRect.content;
        }

        // Find TMP under content if not set
        if (dialogueText == null && content != null)
            dialogueText = content.GetComponentInChildren<TextMeshProUGUI>(true);

        // Fallback: find viewport/content by common names
        if (viewport == null)
        {
            var t = transform.Find("Viewport");
            if (t != null) viewport = t as RectTransform;
        }

        if (content == null && viewport != null)
        {
            var t = viewport.Find("Content");
            if (t != null) content = t as RectTransform;
        }
    }

    /// <summary>Replace all text.</summary>
    public void SetText(string text)
    {
        EnsureWired();
        bool shouldStick = ShouldStickToBottom();

        dialogueText.text = text;

        KickScroll(shouldStick);
    }

    /// <summary>Append a line (adds newline automatically if needed).</summary>
    public void AppendLine(string line)
    {
        EnsureWired();
        bool shouldStick = ShouldStickToBottom();

        if (!string.IsNullOrEmpty(dialogueText.text))
            dialogueText.text += "\n";
        dialogueText.text += line;

        KickScroll(shouldStick);
    }

    /// <summary>Append raw text (no newline injected).</summary>
    public void Append(string text)
    {
        EnsureWired();
        bool shouldStick = ShouldStickToBottom();

        dialogueText.text += text;

        KickScroll(shouldStick);
    }

    /// <summary>
    /// Call this when external code mutates the dialogue text directly.
    /// It will decide whether to stick and perform the scroll/layout refresh.
    /// </summary>
    public void NudgeAfterExternalChange()
    {
        EnsureWired();
        KickScroll(ShouldStickToBottom());
    }

    private void EnsureWired()
    {
        if (scrollRect == null || viewport == null || content == null || dialogueText == null)
            AutoWire();
    }

    private bool ShouldStickToBottom()
    {
        if (!stickToBottomOnly) return true;
        if (scrollRect == null) return true;

        // verticalNormalizedPosition: 1 = top, 0 = bottom
        return scrollRect.verticalNormalizedPosition <= bottomThreshold;
    }

    private void KickScroll(bool shouldScrollToBottom)
    {
        if (_scrollCo != null) StopCoroutine(_scrollCo);
        _scrollCo = StartCoroutine(RebuildAndMaybeScroll(shouldScrollToBottom));
    }

    private IEnumerator RebuildAndMaybeScroll(bool shouldScrollToBottom)
    {
        // TMP + layout groups settle over frames; do 2 passes to be robust.
        yield return null;
        yield return new WaitForEndOfFrame();

        ForceLayoutNow();

        // If content is not bigger than viewport, scrolling is meaningless.
        bool canScroll = content.rect.height > viewport.rect.height + 0.5f;

        if (shouldScrollToBottom && canScroll)
        {
            // Kill inertia fighting the set
            scrollRect.velocity = Vector2.zero;

            // Force bottom. Set BOTH to avoid Unity quirks.
            scrollRect.verticalNormalizedPosition = 0f;
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
        }

        // One more pass prevents "snaps back" on some UI stacks.
        yield return null;
        ForceLayoutNow();

        if (shouldScrollToBottom && canScroll)
        {
            scrollRect.velocity = Vector2.zero;
            scrollRect.verticalNormalizedPosition = 0f;
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
        }

        _scrollCo = null;
    }

    private void ForceLayoutNow()
    {
        if (dialogueText != null) dialogueText.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();

        if (content != null) LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (viewport != null) LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);

        Canvas.ForceUpdateCanvases();
    }
}
