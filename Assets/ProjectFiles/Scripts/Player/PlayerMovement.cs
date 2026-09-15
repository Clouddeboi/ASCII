using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    float coyoteTimeCounter;
    float jumpBufferCounter;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    bool wasGrounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slopeStickForce = 80f;
    [SerializeField] private float slideForce = 20f;
    [SerializeField] private float slideControlMultiplier = 0.3f;
    [SerializeField] private float maxSlideSpeed = 40f;
    [SerializeField] private float slopeCheckRadius = 0.25f;
    [SerializeField] private bool debugSlopeLogging = false;
    RaycastHit slopeHit;
    bool onSlope;
    bool onSteepSlope;
    bool exitingSlope;

    [Header("Animation")]
    [SerializeField] private PlayerHandAnimator handAnimator;
    [SerializeField] private float runAnimationSpeedThreshold = 0.5f;
    [SerializeField] private float fallVelocityThreshold = -1f;

    [Header("Audio")]
    [SerializeField] private PlayerAudioController audioController;

    [Header("Landing Camera Shake")]
    [SerializeField] private float hardLandingFallSpeed = 10f;
    [SerializeField] private float hardLandingTrauma = 0.5f;
    private float fallSpeedBeforeLanding;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    //Exposed for camera effects (head bob) and other systems that need locomotion state.
    public bool IsGrounded => grounded;
    public float FlatSpeed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
    public float MoveSpeed => moveSpeed;
    public float VerticalVelocity => rb.linearVelocity.y;
    public bool IsOnSteepSlope => onSteepSlope;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        wasGrounded = true;
    }

    //Disabling this script (e.g. entering Terminal state) stops FixedUpdate/drag, so nothing would otherwise counter existing horizontal velocity - cancel just the horizontal drift so the player doesn't keep sliding, without freezing gravity/vertical motion (a jump/fall completes naturally).
    private void OnDisable()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        UpdateAnimationState();
        audioController?.UpdateFootsteps(grounded, FlatSpeed, moveSpeed);

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        wasGrounded = grounded;
    }

    private void UpdateAnimationState()
    {
        if (!grounded)
            fallSpeedBeforeLanding = Mathf.Max(fallSpeedBeforeLanding, -rb.linearVelocity.y);

        //Just landed after being airborne
        if (grounded && !wasGrounded)
        {
            bool hardLanding = fallSpeedBeforeLanding >= hardLandingFallSpeed;
            if (hardLanding)
                PlayerCameraEffects.Instance?.AddTrauma(hardLandingTrauma);

            audioController?.PlayLand(hardLanding);

            fallSpeedBeforeLanding = 0f;
        }

        if (handAnimator == null) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMovingOnGround = grounded && flatVel.magnitude > runAnimationSpeedThreshold;
        handAnimator.SetRunning(isMovingOnGround);

        handAnimator.SetGrounded(grounded);

        bool isFalling = !grounded && rb.linearVelocity.y < fallVelocityThreshold;
        handAnimator.SetFalling(isFalling);

        if (grounded && !wasGrounded)
            handAnimator.SetFalling(false);
    }

    private void FixedUpdate()
    {
        CheckSlope();
        MovePlayer();
    }

    //SphereCasts down from the player to classify the ground as flat/walkable slope/too-steep-to-stand-on.
    //A sphere (rather than a thin ray) avoids false "steep" readings from catching a stair riser's near-vertical edge.
    private void CheckSlope()
    {
        if (Physics.SphereCast(transform.position, slopeCheckRadius, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = angle <= maxSlopeAngle && angle != 0f;
            onSteepSlope = angle > maxSlopeAngle;

            if (debugSlopeLogging)
                Debug.Log($"[Slope] angle={angle:F1} onSlope={onSlope} onSteepSlope={onSteepSlope} grounded={grounded} speed={rb.linearVelocity.magnitude:F2} vel={rb.linearVelocity}");
        }
        else
        {
            onSlope = false;
            onSteepSlope = false;

            if (debugSlopeLogging)
                Debug.Log($"[Slope] no ground hit - grounded={grounded} speed={rb.linearVelocity.magnitude:F2}");
        }
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void MyInput()
    {
        Vector2 moveInput = InputManager.Instance.MoveAction.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        //coyote time, still allowed to jump for a short window after walking off a ledge
        if (grounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        //jump buffer, a press slightly before landing is remembered instead of dropped
        if (InputManager.Instance.JumpAction.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        //requires a fresh press (WasPressedThisFrame), holding the button will not repeat the jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && readyToJump)
        {
            readyToJump = false;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //sliding down a slope too steep to walk on - player retains only partial control
        if (onSteepSlope && grounded && !exitingSlope)
        {
            Vector3 slideDirection = GetSlopeMoveDirection(Vector3.down);
            rb.AddForce(slideDirection * slideForce, ForceMode.Force);
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * slideControlMultiplier, ForceMode.Force);
            return;
        }

        //walking on a walkable slope/stairs - project movement onto the slope so we walk along it, not into/off it
        if (onSlope && grounded && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 10f, ForceMode.Force);

            //Keeps the player stuck to the slope's surface instead of hopping when moving downhill
            if (rb.linearVelocity.y > 0f)
                rb.AddForce(Vector3.down * slopeStickForce, ForceMode.Force);

            //Cancels gravity's component running down along the slope surface so walkable slopes/stairs don't slowly slide the player downhill
            Vector3 gravityAlongSlope = Vector3.ProjectOnPlane(Physics.gravity, slopeHit.normal);
            rb.AddForce(-gravityAlongSlope, ForceMode.Acceleration);

            return;
        }

        //on flat ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        //sliding down a steep slope should be allowed to build up well past normal walking speed - cap separately instead of falling through to the walk-speed clamp below
        if (onSteepSlope && grounded && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > maxSlideSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSlideSpeed;
            return;
        }

        //on a walkable slope, limit the full velocity (including vertical) so slopes don't let us exceed moveSpeed
        if (onSlope && grounded && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        handAnimator?.PlayJump();
        audioController?.PlayJump();
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
}