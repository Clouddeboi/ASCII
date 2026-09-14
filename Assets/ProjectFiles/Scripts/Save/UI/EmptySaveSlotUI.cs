using System;
using UnityEngine;
using UnityEngine.UI;

//The "create a new save" entry shown in the save-slot list (in place of a real slot). Clicking it creates
//an auto-named save ("Save (x)") and starts a new game - the player never types a save name.
public class EmptySaveSlotUI : MonoBehaviour
{
    [SerializeField] private Button createButton;

    public void Init(Action onCreateClicked)
    {
        if (createButton == null)
        {
            Debug.LogWarning("[EmptySaveSlotUI] Create Button not assigned in the Inspector - clicks will do nothing.", this);
            return;
        }

        createButton.onClick.AddListener(() => onCreateClicked?.Invoke());
    }
}
