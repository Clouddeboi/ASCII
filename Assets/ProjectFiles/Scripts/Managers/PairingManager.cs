using System.Collections.Generic;
using UnityEngine;

//Data-driven entity pairing/override system. Only Player->Suicide is unlocked at game start.
public class PairingManager : MonoBehaviour
{
    public static PairingManager Instance { get; private set; }

    [SerializeField] private PairingData pairingData;

    //Index 0 in pairingData is expected to be the Player -> Suicide pairing.
    [SerializeField] private int suicidePairingIndex = 0;

    private HashSet<int> unlockedIndices = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pairingData != null && pairingData.pairings != null)
        {
            for (int i = 0; i < pairingData.pairings.Length; i++)
            {
                if (pairingData.pairings[i].unlockedByDefault)
                    unlockedIndices.Add(i);
            }
        }
    }

    public PairingData.PairingEntry[] GetAllPairings() => pairingData != null ? pairingData.pairings : null;

    public bool IsUnlocked(int index) => unlockedIndices.Contains(index);

    public void UnlockPairing(int index)
    {
        if (pairingData == null || pairingData.pairings == null) return;
        if (index < 0 || index >= pairingData.pairings.Length) return;
        unlockedIndices.Add(index);
    }

    //Returns a message describing the result; execution is only supported for the wired suicide pairing.
    public string ExecutePairing(int index)
    {
        if (pairingData == null || pairingData.pairings == null)
            return "OVERRIDE SYSTEM UNAVAILABLE.";

        if (index < 0 || index >= pairingData.pairings.Length)
            return "INVALID PAIRING INDEX.";

        if (!IsUnlocked(index))
            return "PAIRING LOCKED.";

        if (index == suicidePairingIndex)
        {
            if (PlayerHealth.Instance == null)
                return "TARGET UNAVAILABLE.";

            PlayerHealth.Instance.Kill();
            return $"PAIRING EXECUTED: {pairingData.pairings[index].displayName}";
        }

        //Other pairings have no wired target yet (entities not implemented).
        return "PAIRING TARGET NOT YET IMPLEMENTED.";
    }
}
