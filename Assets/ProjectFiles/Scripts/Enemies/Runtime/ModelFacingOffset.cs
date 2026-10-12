using UnityEngine;

// Corrects clips/imports whose baked forward axis differs from the enemy's; assign any transform to rotate.
public class ModelFacingOffset : MonoBehaviour
{
    [Tooltip("Transform to rotate. Defaults to this object if left empty.")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 eulerOffset = new Vector3(0f, 180f, 0f);

    private void Awake()
    {
        if (target == null)
            target = transform;

        Apply();
    }

    // Absolute assignment (not cumulative), so re-applying each frame is safe and wins over the Animator.
    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        target.localRotation = Quaternion.Euler(eulerOffset);
    }
}
