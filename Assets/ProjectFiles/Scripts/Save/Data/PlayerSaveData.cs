using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public float currentHealth;
    public float maxHealth;
    public Vector3 position;
    public Quaternion rotation;
    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    public bool hasCheckpoint;
    public string checkpointId;
    public Vector3 checkpointPosition;
    public Quaternion checkpointRotation;
}
