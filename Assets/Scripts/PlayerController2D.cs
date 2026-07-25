using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerSensors2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerSensors2D sensors;

    [Header("Animation & Visuals")]
    [SerializeField] private Animator anim;
    [SerializeField] private string idleAnimationName = "Idle_Clip";
    [SerializeField] private string runAnimationName = "Run_Clip";
    [SerializeField] private string jumpAnimationName = "Jump_Clip";
    [SerializeField] private string fallAnimationName = "Fall_Clip";
    [SerializeField] private Transform visualRoot;
    [SerializeField] private ParticleSystem runTrail;
    [SerializeField] private float maxLeanAngle = 10f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float baseFootstepInterval = 0.4f;

    [Header("Move (Momentum Based)")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float groundAcceleration = 70f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float airAcceleration = 45f;
    [SerializeField] private float airDeceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpCooldown = 0.05f;

    [Header("Gravity (Realistic Arc)")]
    [SerializeField] private float baseGravity = 4f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.4f;
    [SerializeField] private float apexBonusMultiplier = 0.5f;
    [SerializeField] private float maxFallSpeed = -20f;

    [Header("Advanced Wall Tech")]
    [SerializeField] private Vector2 wallHopForce = new Vector2(6f, 13f);
    [SerializeField] private Vector2 wallLeapForce = new Vector2(14f, 12f);
    [SerializeField] private float wallJumpLockTime = 0.15f;
    [SerializeField] private float wallCoyoteTime = 0.15f;
    [SerializeField] private float wallSlideMaxSpeed = -3f;

    public float MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public int Facing => facing;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float wallCoyoteCounter;
    private int lastWallSide;
    private float wallJumpLockCounter;
    private float jumpCooldownCounter;
    private int facing = 1;
    private bool wasGroundedLastFrame = false;
    private float footstepTimer = 0f;

    private PlayerCombatOrParry combat;

    private enum AnimState { None, Idle, Run, Jump, Fall }
    private AnimState currentAnimState = AnimState.None;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sensors == null) sensors = GetComponent<PlayerSensors2D>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        combat = GetComponent<PlayerCombatOrParry>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GameplayLocked)
        {
            MoveInput = 0f;
            JumpHeld = false;
            JumpPressedThisFrame = false;
            JumpReleasedThisFrame = false;
            if (runTrail != null) runTrail.Stop();
            return;
        }

        UpdateFacing();
        sensors.SetFacing(facing);
        sensors.Tick();

        HandleLandingAudio();
        HandleFootstepsAudio();

        UpdateTimers();
        HandleJumpInput();
        HandleJumpCut();
        UpdateAnimations();

        JumpPressedThisFrame = false;
        JumpReleasedThisFrame = false;
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GameplayLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        ApplyHorizontalMovement();
        ApplyBetterGravity();
        ApplyWallSlide();
        ApplyLeaning();
    }

    private void HandleLandingAudio()
    {
        if (!wasGroundedLastFrame && sensors.IsGrounded)
        {
            if (playerAudioSource != null && landSound != null)
            {
                playerAudioSource.PlayOneShot(landSound);
            }
        }
        wasGroundedLastFrame = sensors.IsGrounded;
    }

    private void HandleFootstepsAudio()
    {
        if (sensors.IsGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
            float speedPercent = currentSpeed / Mathf.Max(moveSpeed, 0.1f);
            float currentInterval = baseFootstepInterval / Mathf.Max(speedPercent, 0.2f);

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= currentInterval)
            {
                footstepTimer = 0f;
                if (playerAudioSource != null && footstepSound != null)
                {
                    playerAudioSource.PlayOneShot(footstepSound);
                }
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void UpdateFacing()
    {
        if (MoveInput > 0.01f) facing = 1;
        else if (MoveInput < -0.01f) facing = -1;

        if (visualRoot != null)
        {
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            visualRoot.localScale = scale;
        }
    }

    private void UpdateTimers()
    {
        if (sensors.IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (sensors.IsTouchingWall)
        {
            wallCoyoteCounter = wallCoyoteTime;
            lastWallSide = sensors.WallSide != 0 ? sensors.WallSide : facing;
        }
        else
        {
            wallCoyoteCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
        if (wallJumpLockCounter > 0f) wallJumpLockCounter -= Time.deltaTime;
        if (jumpCooldownCounter > 0f) jumpCooldownCounter -= Time.deltaTime;
    }

    private void HandleJumpInput()
    {
        if (jumpCooldownCounter > 0f) return;
        if (jumpBufferCounter <= 0f) return;

        bool canWallJump = !sensors.IsGrounded && wallCoyoteCounter > 0f;
        bool canGroundJump = coyoteCounter > 0f;

        if (canGroundJump)
        {
            DoGroundJump();
        }
        else if (canWallJump)
        {
            DoWallJump();
        }
    }

    private void HandleJumpCut()
    {
        if (JumpReleasedThisFrame && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteCounter = 0f;
        }
    }

    private void ApplyHorizontalMovement()
    {
        if (wallJumpLockCounter > 0f) return;

        bool grounded = sensors.IsGrounded;
        float targetSpeed = MoveInput * moveSpeed;
        float currentSpeed = rb.linearVelocity.x;

        bool accelerating = Mathf.Abs(targetSpeed) > 0.01f;
        float rate = accelerating
            ? (grounded ? groundAcceleration : airAcceleration)
            : (grounded ? groundDeceleration : airDeceleration);

        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    private void ApplyBetterGravity()
    {
        bool isAtApex = Mathf.Abs(rb.linearVelocity.y) < 2f && !sensors.IsGrounded;

        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0f && !JumpHeld)
            rb.gravityScale = baseGravity * lowJumpGravityMultiplier;
        else if (isAtApex)
            rb.gravityScale = baseGravity * apexBonusMultiplier;
        else
            rb.gravityScale = baseGravity;

        if (rb.linearVelocity.y < maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
    }

    private void ApplyWallSlide()
    {
        if (sensors.IsGrounded || !sensors.IsTouchingWall) return;
        if (wallJumpLockCounter > 0f) return;
        if (rb.linearVelocity.y >= 0f) return;

        int wallSide = sensors.WallSide != 0 ? sensors.WallSide : lastWallSide;
        bool pressingIntoWall = (wallSide > 0 && MoveInput > 0.01f) || (wallSide < 0 && MoveInput < -0.01f);
        if (!pressingIntoWall) return;

        if (rb.linearVelocity.y < wallSlideMaxSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideMaxSpeed);
    }

    private void ApplyLeaning()
    {
        if (visualRoot == null) return;

        float speedRatio = Mathf.Clamp(rb.linearVelocity.x / moveSpeed, -1f, 1f);
        float targetZRotation = speedRatio * -maxLeanAngle;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        visualRoot.localRotation = Quaternion.Lerp(visualRoot.localRotation, targetRotation, Time.deltaTime * leanSpeed);
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        if (combat != null && combat.IsParryActive) return;

        AnimState desired;
        if (!sensors.IsGrounded)
        {
            desired = rb.linearVelocity.y > 0.01f ? AnimState.Jump : AnimState.Fall;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            desired = AnimState.Run;
        }
        else
        {
            desired = AnimState.Idle;
        }

        if (desired != currentAnimState)
        {
            currentAnimState = desired;

            switch (desired)
            {
                case AnimState.Jump:
                    string jumpClip = !string.IsNullOrEmpty(jumpAnimationName) ? jumpAnimationName : fallAnimationName;
                    if (!string.IsNullOrEmpty(jumpClip)) anim.Play(jumpClip);
                    if (runTrail != null) runTrail.Stop();
                    break;
                case AnimState.Fall:
                    string fallClip = !string.IsNullOrEmpty(fallAnimationName) ? fallAnimationName : jumpAnimationName;
                    if (!string.IsNullOrEmpty(fallClip)) anim.Play(fallClip);
                    if (runTrail != null) runTrail.Stop();
                    break;
                case AnimState.Run:
                    if (!string.IsNullOrEmpty(runAnimationName)) anim.Play(runAnimationName);
                    if (runTrail != null) runTrail.Play();
                    break;
                case AnimState.Idle:
                    if (!string.IsNullOrEmpty(idleAnimationName)) anim.Play(idleAnimationName);
                    if (runTrail != null) runTrail.Stop();
                    break;
            }
        }
    }

    private void DoGroundJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
    }

    private void DoWallJump()
    {
        int wallDir = lastWallSide != 0 ? lastWallSide : facing;
        Vector2 force;

        if (MoveInput != 0 && Mathf.Sign(MoveInput) == wallDir)
        {
            force = new Vector2(-wallDir * wallHopForce.x, wallHopForce.y);
            wallJumpLockCounter = wallJumpLockTime;
        }
        else
        {
            force = new Vector2(-wallDir * wallLeapForce.x, wallLeapForce.y);
            wallJumpLockCounter = wallJumpLockTime;
        }

        rb.linearVelocity = force;
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        wallCoyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
        facing = -wallDir;
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        MoveInput = input.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpHeld = true;
            JumpPressedThisFrame = true;
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            JumpHeld = false;
            JumpReleasedThisFrame = true;
        }
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}