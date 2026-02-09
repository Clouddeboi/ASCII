using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("NPC Identity")]
    public string npcName = "Unnamed NPC";
    
    [Header("Behavior Settings")]
    public float responseDelay = 0.5f;
    public float responseCooldown = 2f;
    
    [Header("Dialogue Responses")]
    public DialogueCategory[] dialogueCategories;
    public string[] defaultResponses = { "Interesting.", "I see.", "Hmm." };
}

[Serializable]
public class DialogueCategory
{
    public string categoryName = "New Category";
    
    [Tooltip("Keywords that trigger this category (not case-sensitive)")]
    public string[] triggers;
    
    [Tooltip("Possible responses for this category")]
    public string[] responses;
    
    [Tooltip("Should this check for exact word match or just contains?")]
    public bool exactWordMatch = false;
}