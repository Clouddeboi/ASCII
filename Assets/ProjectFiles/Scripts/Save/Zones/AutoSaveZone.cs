using UnityEngine;

//Reusable trigger volume, drop into a scene, resize its Collider, leave isTrigger checked.
//Any object tagged "Player" entering it requests an auto-save via SaveManager (debounced internally).
[RequireComponent(typeof(Collider))]
public class AutoSaveZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string autoSaveReason = "zone";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        SaveManager.Instance?.RequestAutoSave(autoSaveReason);
    }
}
