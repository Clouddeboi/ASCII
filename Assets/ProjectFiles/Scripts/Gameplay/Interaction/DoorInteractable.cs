using UnityEngine;

//Example interactable: toggles open/closed, driven by an Animator bool. Persists isOpen via ISaveable.
public class DoorInteractable : InteractableBase, ISaveable
{
    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openParam = "Open";
    [SerializeField] private string saveId;

    private bool isOpen;

    public string SaveId => saveId;

    protected override void Awake()
    {
        base.Awake();
        SaveableRegistry.Instance?.Register(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveableRegistry.Instance?.Unregister(this);
    }

    protected override bool PerformAction(string[] args)
    {
        SetOpen(!isOpen);
        return true;
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        if (doorAnimator != null)
            doorAnimator.SetBool(openParam, isOpen);
    }

    public string CaptureState() => JsonUtility.ToJson(new DoorState { isOpen = isOpen });

    public void RestoreState(string json)
    {
        DoorState state = JsonUtility.FromJson<DoorState>(json);
        SetOpen(state.isOpen);
    }

    [System.Serializable]
    private struct DoorState
    {
        public bool isOpen;
    }
}
