using UnityEngine;

public interface IListener
{
    void OnPlayerSpoke(GameObject speaker, string spokenText);
}
