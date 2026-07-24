using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerSensors2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerSensors2D sensors;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float airControl = 0.85f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpCooldown = 0.05f;

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 4f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.4f;
    [SerializeField] private float maxFallSpeed = -20f;

    [Header("Wall")]
    [SerializeField] private float wallSlideSpeed = -2f;
    [SerializeField] private float wallJumpX = 10f;
    [SerializeField] private float wallJumpY = 14f;
    [SerializeField] private float wallJumpLockTime = 0.15f;
    [SerializeField] private float wallRegrabBlockTime = 0.12f;

    public float MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public bool IsWallSliding { get; private set; }
    public int Facing => facing;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float wallJumpLockCounter;
    private float wallRegrabBlockCounter;
    private float jumpCooldownCounter;
    private int facing = 1;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (sensors == null)
            sensors = GetComponent<PlayerSensors2D>();

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
            return;
        }

        UpdateFacing();
        sensors.SetFacing(facing);
        sensors.Tick();

        UpdateTimers();
        UpdateWallSlideState();
        HandleJumpInput();
        HandleJumpCut();

        JumpPressedThisFrame = false;
        JumpReleasedThisFrame = false;

        //Debug.Log($"Grounded={sensors.IsGrounded}, VelY={rb.linearVelocity.y}, Coyote={coyoteCounter}, Buffer={jumpBufferCounter}");
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GameplayLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        ApplyHorizontalMovement();
        ApplyWallSlide();
        ApplyBetterGravity();
    }

   
    private void UpdateFacing()
    {
        if (MoveInput > 0.01f)
            facing = 1;
        else if (MoveInput < -0.01f)
            facing = -1;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facing;
        transform.localScale = scale;
    }
   

    private void UpdateTimers()
    {
        if (sensors.IsGrounded && rb.linearVelocity.y <= 0.05f)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;

        if (wallJumpLockCounter > 0f)
            wallJumpLockCounter -= Time.deltaTime;

        if (wallRegrabBlockCounter > 0f)
            wallRegrabBlockCounter -= Time.deltaTime;

        if (jumpCooldownCounter > 0f)
            jumpCooldownCounter -= Time.deltaTime;
    }

    private void UpdateWallSlideState()
    {
        bool pushingIntoWall =
            (MoveInput > 0.01f && sensors.WallSide == 1) ||
            (MoveInput < -0.01f && sensors.WallSide == -1);

        IsWallSliding =
            !sensors.IsGrounded &&
            sensors.IsTouchingWall &&
            rb.linearVelocity.y < 0f &&
            pushingIntoWall &&
            wallRegrabBlockCounter <= 0f;
    }

    private void HandleJumpInput()
    {
        if (jumpCooldownCounter > 0f)
            return;

        if (JumpPressedThisFrame && IsWallSliding)
        {
            DoWallJump();
            return;
        }

        bool canGroundJump =
            jumpBufferCounter > 0f &&
            coyoteCounter > 0f &&
            rb.linearVelocity.y <= 0.05f;

        if (canGroundJump)
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
        if (wallJumpLockCounter > 0f)
            return;

        float control = sensors.IsGrounded ? 1f : airControl;
        rb.linearVelocity = new Vector2(MoveInput * moveSpeed * control, rb.linearVelocity.y);
    }

    private void ApplyWallSlide()
    {
        if (!IsWallSliding)
            return;

        float clampedY = Mathf.Max(rb.linearVelocity.y, wallSlideSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    private void DoGroundJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
        IsWallSliding = false;
        JumpPressedThisFrame = false;
    }

    private void DoWallJump()
    {
        int wallSide = sensors.WallSide == 0 ? facing : sensors.WallSide;

        rb.linearVelocity = new Vector2(-wallSide * wallJumpX, wallJumpY);

        wallJumpLockCounter = wallJumpLockTime;
        wallRegrabBlockCounter = wallRegrabBlockTime;
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCooldownCounter = jumpCooldown;
        IsWallSliding = false;
        JumpHeld = true;
        JumpPressedThisFrame = false;
    }

    private void ApplyBetterGravity()
    {
        rb.gravityScale = baseGravity;

        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0f && !JumpHeld)
            rb.gravityScale = baseGravity * lowJumpGravityMultiplier;

        if (rb.linearVelocity.y < maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
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