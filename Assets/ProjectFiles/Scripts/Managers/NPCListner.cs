using UnityEngine;
using System.Collections;

public class NPCListener : MonoBehaviour, IListener
{
    [Header("Response Settings")]
    [SerializeField] private Voice npcVoice;
    [SerializeField] private float responseDelay = 0.5f; //Wait a bit before responding
    
    [Header("Responses")]
    [SerializeField] private string[] greetingResponses = { "Hello there!", "Hi friend!", "Greetings!" };
    [SerializeField] private string[] questionResponses = { "I'm not sure.", "Let me think...", "Good question!" };
    [SerializeField] private string[] defaultResponses = { "Interesting.", "I see.", "Hmm." };

    private float lastResponseTime;
    [SerializeField] private float responseCooldown = 2f;

    private Coroutine responseCoroutine;

    public void OnPlayerSpoke(GameObject speaker, string spokenText)
    {
        Debug.Log($"[{gameObject.name}] OnPlayerSpoke called with: '{spokenText}'");
        
        if (Time.time - lastResponseTime < responseCooldown)
        {
            Debug.Log($"[{gameObject.name}] Still in cooldown");
            return;
        }
        
        lastResponseTime = Time.time;

        if (!speaker.transform.root.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Speaker is not player");
            return;
        }

        Debug.Log($"[{gameObject.name}] Starting response coroutine");
        
        if (responseCoroutine != null)
            StopCoroutine(responseCoroutine);

        responseCoroutine = StartCoroutine(RespondAfterDelay(spokenText));
    }

    private IEnumerator RespondAfterDelay(string playerText)
    {
        Debug.Log($"[{gameObject.name}] RespondAfterDelay coroutine STARTED");
        
        yield return new WaitForSeconds(responseDelay);

        //Choose response based on what player said
        string response = ChooseResponse(playerText);
        
        Debug.Log($"{gameObject.name} responding: {response}");
        
        if (npcVoice != null)
        {
            npcVoice.Speak(response);
        }

        responseCoroutine = null;
    }

    private string ChooseResponse(string playerText)
    {
        string textLower = playerText.ToLower();

        //Check for greetings
        if (textLower.Contains("hello") || textLower.Contains("hi") || textLower.Contains("hey"))
        {
            return greetingResponses[Random.Range(0, greetingResponses.Length)];
        }

        //Check for questions
        if (textLower.Contains("?") || textLower.Contains("what") || textLower.Contains("how") || textLower.Contains("why"))
        {
            return questionResponses[Random.Range(0, questionResponses.Length)];
        }

        //Default response
        return defaultResponses[Random.Range(0, defaultResponses.Length)];
    }
}