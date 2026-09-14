using UnityEngine;

//Shared helper so save-load and checkpoint respawn reposition the Rigidbody-based player the same safe way
//(zeroing velocity first so leftover momentum doesn't fling the player on the next physics step).
public static class PlayerTeleportUtility
{
    public static void Teleport(Transform playerTransform, Vector3 position, Quaternion rotation)
    {
        if (playerTransform == null)
            return;

        playerTransform.SetPositionAndRotation(position, rotation);

        //A non-kinematic Rigidbody on a CHILD (e.g. "PlayerObj" under a "Player" root) doesn't reliably follow
        //a moved ancestor transform - PhysX can snap it back to its last simulated position on the next
        //FixedUpdate. Explicitly push the (now-updated-by-the-parent-move) world transform into the Rigidbody
        //itself and force an immediate sync so physics doesn't undo the move.
        var rb = playerTransform.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = rb.transform.position;
            rb.rotation = rb.transform.rotation;
            Physics.SyncTransforms();
        }
    }
}
