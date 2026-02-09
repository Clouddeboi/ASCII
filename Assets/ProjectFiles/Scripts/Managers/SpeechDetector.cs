using UnityEngine;
using System.Collections.Generic;

public class SpeechDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float hearingRadius = 10f;
    [SerializeField] private LayerMask listenerLayer; //only detect specific layers
    
    private SphereCollider hearingCollider;
    private HashSet<IListener> nearbyListeners = new HashSet<IListener>();

    void Awake()
    {
        //Create hearing sphere collider
        hearingCollider = gameObject.AddComponent<SphereCollider>();
        hearingCollider.radius = hearingRadius;
        hearingCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((listenerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        //Check if object can listen
        IListener listener = other.GetComponentInParent<IListener>();
        if (listener != null)
        {
            nearbyListeners.Add(listener);
            //Debug.Log($"Listener entered range: {other.gameObject.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((listenerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        //Remove listener when out of range
        IListener listener = other.GetComponentInParent<IListener>();
        if (listener != null)
        {
            nearbyListeners.Remove(listener);
            //Debug.Log($"Listener left range: {other.gameObject.name}");
        }
    }

    public void NotifySpeech(GameObject speaker, string spokenText )
    {
        //Debug.Log($"Player spoke: '{spokenText}' - Notifying {nearbyListeners.Count} listeners");
        
        //Notify all nearby listeners
        foreach (IListener listener in nearbyListeners)
        {
            if (((MonoBehaviour)listener).gameObject == speaker)
                continue;

            listener.OnPlayerSpoke(speaker, spokenText);
        }
    }

    //Visualize hearing range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}