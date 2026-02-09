using UnityEngine;
using System.Collections;
using System.Linq;

public class NPCListener : MonoBehaviour, IListener
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCData npcData;
    
    [Header("Component References")]
    [SerializeField] private Voice npcVoice;

    private float lastResponseTime;
    private Coroutine responseCoroutine;

    void Start()
    {
        if (npcData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No NPCData assigned!");
        }
    }

    public void OnPlayerSpoke(GameObject speaker, string spokenText)
    {
        if (npcData == null) return;
        
        Debug.Log($"[{npcData.npcName}] OnPlayerSpoke called with: '{spokenText}'");
        
        if (Time.time - lastResponseTime < npcData.responseCooldown)
        {
            Debug.Log($"[{npcData.npcName}] Still in cooldown");
            return;
        }
        
        lastResponseTime = Time.time;

        if (!speaker.transform.root.CompareTag("Player"))
        {
            Debug.Log($"[{npcData.npcName}] Speaker is not player");
            return;
        }

        Debug.Log($"[{npcData.npcName}] Starting response coroutine");
        
        if (responseCoroutine != null)
            StopCoroutine(responseCoroutine);

        responseCoroutine = StartCoroutine(RespondAfterDelay(spokenText));
    }

    private IEnumerator RespondAfterDelay(string playerText)
    {
        Debug.Log($"[{npcData.npcName}] RespondAfterDelay coroutine STARTED");
        
        yield return new WaitForSeconds(npcData.responseDelay);

        string response = ChooseResponse(playerText);
        
        Debug.Log($"{npcData.npcName} responding: {response}");
        
        if (npcVoice != null)
        {
            npcVoice.Speak(response);
        }

        responseCoroutine = null;
    }

    private string ChooseResponse(string playerText)
    {
        string textLower = playerText.ToLower();

        // Check each dialogue category
        foreach (DialogueCategory category in npcData.dialogueCategories)
        {
            if (category.triggers == null || category.triggers.Length == 0)
                continue;

            bool triggered = false;

            foreach (string trigger in category.triggers)
            {
                if (string.IsNullOrEmpty(trigger))
                    continue;

                string triggerLower = trigger.ToLower();

                if (category.exactWordMatch)
                {
                    // Check for exact word match
                    string[] words = textLower.Split(new char[] { ' ', '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
                    triggered = words.Contains(triggerLower);
                }
                else
                {
                    // Check if trigger is contained anywhere
                    triggered = textLower.Contains(triggerLower);
                }

                if (triggered)
                    break;
            }

            // If triggered and has responses, return random response
            if (triggered && category.responses != null && category.responses.Length > 0)
            {
                return category.responses[Random.Range(0, category.responses.Length)];
            }
        }

        // Return default response if no category matched
        if (npcData.defaultResponses != null && npcData.defaultResponses.Length > 0)
        {
            return npcData.defaultResponses[Random.Range(0, npcData.defaultResponses.Length)];
        }

        return "...";
    }
}