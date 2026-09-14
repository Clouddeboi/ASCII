using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Reusable Yes/No popup. Wire confirmButton/cancelButton's OnClick to OnConfirmClicked/OnCancelClicked in the
//Inspector, then call Show(...) from any script that needs a confirmation before a destructive action
//(overwriting a save, deleting a save, loading over unsaved progress, etc).
public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show(string message, Action onConfirmed, Action onCancelled = null)
    {
        onConfirm = onConfirmed;
        onCancel = onCancelled;

        if (messageText != null)
            messageText.text = message;

        if (root == null)
        {
            Debug.LogError("[ConfirmationDialog] Root not assigned in the Inspector - popup can't be shown.", this);
            return;
        }

        root.SetActive(true);

        if (!root.activeInHierarchy)
            Debug.LogWarning("[ConfirmationDialog] Root was activated but a parent GameObject is inactive, so it still won't be visible.", root);
    }

    public void OnConfirmClicked()
    {
        Hide();
        onConfirm?.Invoke();
    }

    public void OnCancelClicked()
    {
        Hide();
        onCancel?.Invoke();
    }

    private void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
