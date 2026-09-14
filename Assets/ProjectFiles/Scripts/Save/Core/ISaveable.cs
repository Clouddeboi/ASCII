//Implemented by any world object that needs its state persisted (doors, future levers/chests/NPCs, etc).
//CaptureState/RestoreState use each implementer's own concrete state type internally (via JsonUtility),
//so the save system only ever handles opaque strings and never depends on individual gameplay scripts.
public interface ISaveable
{
    //Stable, inspector-assigned identifier - must be unique per scene and never change once placed.
    string SaveId { get; }

    string CaptureState();
    void RestoreState(string json);
}
