using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //Mouse Delta from the Input System is already a per-frame delta; multiplying by Time.deltaTime
        //on top of it double-dips, so any frame hitch (e.g. spiked deltaTime while a UI prompt toggles)
        //produces a visible jolt instead of a smooth turn.
        Vector2 lookInput = InputManager.Instance.LookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * sensX;
        float mouseY = lookInput.y * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
