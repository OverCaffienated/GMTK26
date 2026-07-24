using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerSensors2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerSensors2D sensors;

    [Header("Animation & Visuals")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private ParticleSystem runTrail;
    [SerializeField] private float maxLeanAngle = 10f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float airControl = 0.85f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpCooldown = 0.05f;
    [SerializeField] private float jumpWindupDelay = 0.08f;

    [Header("Gravity (Realistic Arc)")]
    [SerializeField] private float baseGravity = 4f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.4f;
    [SerializeField] private float apexBonusMultiplier = 0.5f;
    [SerializeField] private float maxFallSpeed = -20f;

    [Header("Wall")]
    [SerializeField] private float wallJumpX = 14f;
    [SerializeField] private float wallJumpY = 12f;
    [SerializeField] private float wallJumpLockTime = 0.35f;

    public float MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public int Facing => facing;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float wallJumpLockCounter;
    private float jumpCooldownCounter;
    private float jumpWindupCounter;
    private bool isWaitingToJump = false;
    private int facing = 1;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sensors == null) sensors = GetComponent<PlayerSensors2D>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
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
        ApplyLeaning();
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

        if (runTrail != null)
        {
            var shape = runTrail.shape;
        }
    }

    private void UpdateTimers()
    {
        if (sensors.IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
        if (wallJumpLockCounter > 0f) wallJumpLockCounter -= Time.deltaTime;
        if (jumpCooldownCounter > 0f) jumpCooldownCounter -= Time.deltaTime;
        if (jumpWindupCounter > 0f) jumpWindupCounter -= Time.deltaTime;
    }

    private void HandleJumpInput()
    {
        if (jumpCooldownCounter > 0f || isWaitingToJump) return;

        if (JumpPressedThisFrame && !sensors.IsGrounded && sensors.IsTouchingWall)
        {
            DoWallJump();
            return;
        }

        bool canGroundJump = jumpBufferCounter > 0f && coyoteCounter > 0f;

        if (canGroundJump && !isWaitingToJump)
        {
            StartCoroutine(DelayedJumpRoutine());
        }
    }

    private IEnumerator DelayedJumpRoutine()
    {
        isWaitingToJump = true;
        jumpWindupCounter = jumpWindupDelay;

        yield return new WaitForSeconds(jumpWindupDelay);

        isWaitingToJump = false;
        DoGroundJump();
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

        float control = sensors.IsGrounded ? 1f : airControl;
        rb.linearVelocity = new Vector2(MoveInput * moveSpeed * control, rb.linearVelocity.y);
    }

    private void ApplyBetterGravity()
    {
        bool isAtApex = Mathf.Abs(rb.linearVelocity.y) < 2f && !sensors.IsGrounded && JumpHeld;

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

    private void ApplyLeaning()
    {
        if (visualRoot == null) return;

        float speedRatio = rb.linearVelocity.x / moveSpeed;
        float targetZRotation = speedRatio * -maxLeanAngle;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        visualRoot.localRotation = Quaternion.Lerp(visualRoot.localRotation, targetRotation, Time.deltaTime * leanSpeed);
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        if (!sensors.IsGrounded)
        {
            anim.Play("Jump");
            if (runTrail != null) runTrail.Stop();
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            anim.Play("Run");
            if (runTrail != null && !runTrail.isPlaying) runTrail.Play();
        }
        else
        {
            anim.Play("Idle");
            if (runTrail != null) runTrail.Stop();
        }
    }

    private void DoGroundJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
        JumpPressedThisFrame = false;
    }

    private void DoWallJump()
    {
        int wallSide = sensors.WallSide == 0 ? facing : sensors.WallSide;
        rb.linearVelocity = new Vector2(-wallSide * wallJumpX, wallJumpY);
        wallJumpLockCounter = wallJumpLockTime;
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
        JumpHeld = true;
        JumpPressedThisFrame = false;
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
}