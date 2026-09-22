using UnityEngine;

//Periodically checks for nearby anomaly-detectable enemies and reports the current danger level.
public class AnomalyMonitor : MonoBehaviour
{
    [SerializeField] private TerminalOutput output;
    [SerializeField] private TerminalAudio terminalAudio;
    [SerializeField] private float scanRadius = 25f;
    [SerializeField] private float checkInterval = 2f;

    private ThreatLevel? currentDanger;
    private float timer;

    public ThreatLevel? CurrentDangerLevel => currentDanger;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        if (EnemyRegistry.Instance == null) return;

        ThreatLevel? danger = EnemyRegistry.Instance.GetHighestAnomalyDanger(transform.position, scanRadius);
        if (danger == currentDanger) return;

        currentDanger = danger;

        if (danger.HasValue)
        {
            output?.PrintSystem($"ANOMALIES DETECTED\n\nDanger Level: {danger.Value.ToString().ToUpperInvariant()}");
            terminalAudio?.PlayResponse();
        }
        else
        {
            output?.PrintSystem("NO ANOMALIES DETECTED.");
        }
    }
}
