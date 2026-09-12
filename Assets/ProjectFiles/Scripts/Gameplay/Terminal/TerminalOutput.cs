using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Renders terminal output, typing system/error/response lines out letter by letter.
public class TerminalOutput : MonoBehaviour
{
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private ScrollRect outputScrollRect;
    [SerializeField] private TerminalConfig config;
    [SerializeField] private TerminalAudio audioController;

    private struct PendingLine
    {
        public string Text;
        public Color? Color;
    }

    private string committedText = "";
    private readonly Queue<PendingLine> pendingLines = new Queue<PendingLine>();
    private Coroutine queueCoroutine;
    private readonly Dictionary<TMP_Text, Coroutine> externalTypeCoroutines = new Dictionary<TMP_Text, Coroutine>();
    private int activeTypingCount;

    public bool IsTyping => queueCoroutine != null;

    public void Clear()
    {
        pendingLines.Clear();
        if (queueCoroutine != null)
        {
            StopCoroutine(queueCoroutine);
            queueCoroutine = null;
            EndTyping();
        }

        committedText = "";
        if (outputText != null)
            outputText.text = "";
    }

    //Echoes the player's typed command instantly, like a real terminal.
    public void Print(string line)
    {
        AppendInstant(line, null);
    }

    public void PrintSystem(string line)
    {
        EnqueueTyped(line, config != null ? config.systemTextColor : Color.green);
    }

    public void PrintError(string line)
    {
        EnqueueTyped(line, config != null ? config.errorTextColor : Color.red);
    }

    public void PrintResponse(string line)
    {
        EnqueueTyped(line, config != null ? config.responseTextColor : Color.cyan);
    }

    //Types text into an unrelated TMP_Text field (e.g. the ASCII art panel), independent of the output log.
    public void TypeIntoText(TMP_Text target, string fullText)
    {
        if (target == null)
            return;

        if (externalTypeCoroutines.TryGetValue(target, out Coroutine existing) && existing != null)
            StopCoroutine(existing);

        externalTypeCoroutines[target] = StartCoroutine(TypeIntoTextRoutine(target, fullText));
    }

    private IEnumerator TypeIntoTextRoutine(TMP_Text target, string fullText)
    {
        float charsPerSecond = config != null ? config.charactersPerSecond : 40f;
        float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;

        BeginTyping();
        StringBuilder partial = new StringBuilder();
        target.text = "";

        foreach (char c in fullText)
        {
            partial.Append(c);
            target.text = partial.ToString();

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }

        externalTypeCoroutines[target] = null;
        EndTyping();
    }

    private void EnqueueTyped(string line, Color? color)
    {
        //Multi-line output is split so each line types out in order, not all at once.
        foreach (string segment in line.Split('\n'))
            pendingLines.Enqueue(new PendingLine { Text = segment, Color = color });

        if (queueCoroutine == null)
            queueCoroutine = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        BeginTyping();

        while (pendingLines.Count > 0)
        {
            PendingLine next = pendingLines.Dequeue();
            yield return TypeLine(next.Text, next.Color);
        }

        queueCoroutine = null;
        EndTyping();
    }

    private IEnumerator TypeLine(string line, Color? color)
    {
        float charsPerSecond = config != null ? config.charactersPerSecond : 40f;
        float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;

        TrimToMaxLines();
        string prefix = committedText.Length > 0 ? committedText + "\n" : "";
        string colorOpen = color.HasValue ? $"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>" : "";
        string colorClose = color.HasValue ? "</color>" : "";

        StringBuilder partial = new StringBuilder();

        foreach (char c in line)
        {
            partial.Append(c);
            if (outputText != null)
                outputText.text = prefix + colorOpen + partial + colorClose;

            ScrollToBottom();

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }

        committedText = prefix + colorOpen + partial + colorClose;
    }

    private void AppendInstant(string line, Color? color)
    {
        string prefix = committedText.Length > 0 ? committedText + "\n" : "";
        string colored = color.HasValue ? $"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>{line}</color>" : line;

        committedText = prefix + colored;
        TrimToMaxLines();

        if (outputText != null)
            outputText.text = committedText;

        ScrollToBottom();
    }

    //Drops the oldest lines once the log exceeds the configured cap.
    private void TrimToMaxLines()
    {
        int maxLines = config != null ? config.maxOutputLines : 200;
        if (maxLines <= 0)
            return;

        string[] lines = committedText.Split('\n');
        if (lines.Length <= maxLines)
            return;

        int skip = lines.Length - maxLines;
        committedText = string.Join("\n", lines, skip, lines.Length - skip);
    }

    private void ScrollToBottom()
    {
        if (outputScrollRect == null)
            return;

        //Rebuild layout immediately so the scroll position reflects the text just added, not last frame's size.
        if (outputScrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(outputScrollRect.content);

        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }

    private void BeginTyping()
    {
        activeTypingCount++;
        if (activeTypingCount == 1)
            audioController?.StartLoading();
    }

    private void EndTyping()
    {
        activeTypingCount = Mathf.Max(0, activeTypingCount - 1);
        if (activeTypingCount == 0)
            audioController?.StopLoading();
    }
}

