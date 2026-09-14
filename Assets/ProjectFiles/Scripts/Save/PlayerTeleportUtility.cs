using UnityEngine;

//Shared helper so save-load and checkpoint respawn reposition the Rigidbody-based player the same safe way
//(zeroing velocity first so leftover momentum doesn't fling the player on the next physics step).
public static class PlayerTeleportUtility
{
    public static void Teleport(Transform playerTransform, Vector3 position, Quaternion rotation)
    {
        if (playerTransform == null)
            return;

        var rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        playerTransform.SetPositionAndRotation(position, rotation);
    }
}
