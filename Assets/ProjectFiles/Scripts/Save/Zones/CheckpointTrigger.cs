using UnityEngine;

//Drop into a scene and shape/size its Collider (kept as a trigger). Any object tagged "Player" entering it
//sets this as the new checkpoint (used for soft respawn on death) and optionally requests an auto-save.
[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private string checkpointId = "checkpoint_01";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerAutoSave = true;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        CheckpointManager.Instance?.SetCheckpoint(checkpointId, transform.position, transform.rotation);

        if (triggerAutoSave)
            SaveManager.Instance?.RequestAutoSave($"checkpoint:{checkpointId}");
    }
}
