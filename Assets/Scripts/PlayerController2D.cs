using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Gravity")]
    [SerializeField] public float baseGravity = 4f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.4f;
    [SerializeField] private float maxFallSpeed = -20f;

    [Header("Wall")]
    [SerializeField] private float wallSlideSpeed = -2f;
    [SerializeField] private float wallJumpX = 10f;
    [SerializeField] private float wallJumpY = 14f;
    [SerializeField] private float wallJumpLockTime = 0.15f;
    [SerializeField] private float wallRegrabBlockTime = 0.12f;

    [Header("Perfect Jump")]
    [SerializeField] private float perfectJumpDistance = 0.18f;
    [SerializeField] private float perfectJumpVerticalBonus = 1.5f;
    [SerializeField] private float perfectJumpHorizontalBonus = 2.5f;
    [SerializeField] private float perfectJumpGroundRayLength = 1.2f;

    public float MoveInput { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public bool IsWallSliding { get; private set; }
    public int Facing => facing;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float wallJumpLockCounter;
    private float wallRegrabBlockCounter;
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
        ReadInput();

        if (MoveInput != 0)
            facing = MoveInput > 0 ? 1 : -1;

        if (sensors != null)
        {
            sensors.SetFacing(facing);
            sensors.Tick();
        }

        UpdateTimers();
        UpdateWallSlideState();
        HandleJumpInput();
        HandleJumpCut();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        ApplyWallSlide();
        ApplyBetterGravity();
    }

    private void ReadInput()
    {
        MoveInput = Input.GetAxisRaw("Horizontal");
        JumpPressedThisFrame = Input.GetButtonDown("Jump");
        JumpReleasedThisFrame = Input.GetButtonUp("Jump");
    }

    private void UpdateTimers()
    {
        if (sensors != null && sensors.IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (JumpPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (wallJumpLockCounter > 0f)
            wallJumpLockCounter -= Time.deltaTime;

        if (wallRegrabBlockCounter > 0f)
            wallRegrabBlockCounter -= Time.deltaTime;
    }

    private void UpdateWallSlideState()
    {
        bool pushingIntoWall = (MoveInput > 0 && facing == 1) || (MoveInput < 0 && facing == -1);

        IsWallSliding =
            sensors != null &&
            !sensors.IsGrounded &&
            sensors.IsTouchingWall &&
            rb.linearVelocity.y < 0f &&
            pushingIntoWall &&
            wallRegrabBlockCounter <= 0f;
    }

    private void HandleJumpInput()
    {
        if (JumpPressedThisFrame && IsWallSliding)
        {
            DoWallJump();
            return;
        }

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            DoGroundJump();
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
        if (wallJumpLockCounter > 0f)
            return;

        float control = sensors != null && sensors.IsGrounded ? 1f : airControl;
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
        float bonusX = 0f;
        float bonusY = 0f;

        IsPerfectEdgeJump(out bonusX, out bonusY);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + bonusX, jumpForce + bonusY);

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        IsWallSliding = false;
    }

    private void DoWallJump()
    {
        int wallDir = facing;

        rb.linearVelocity = new Vector2(-wallDir * wallJumpX, wallJumpY);

        wallJumpLockCounter = wallJumpLockTime;
        wallRegrabBlockCounter = wallRegrabBlockTime;
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        IsWallSliding = false;
    }

    private bool IsPerfectEdgeJump(out float horizontalBonus, out float verticalBonus)
    {
        horizontalBonus = 0f;
        verticalBonus = 0f;

        if (sensors == null)
            return false;

        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, perfectJumpGroundRayLength, sensors.groundLayer);
        if (!groundHit.collider)
            return false;

        Bounds b = groundHit.collider.bounds;
        float distLeft = Mathf.Abs(transform.position.x - b.min.x);
        float distRight = Mathf.Abs(transform.position.x - b.max.x);
        float nearest = Mathf.Min(distLeft, distRight);

        if (nearest > perfectJumpDistance)
            return false;

        float dir = distRight < distLeft ? 1f : -1f;
        horizontalBonus = dir * perfectJumpHorizontalBonus;
        verticalBonus = perfectJumpVerticalBonus;
        return true;
    }

    private void ApplyBetterGravity()
    {
        rb.gravityScale = baseGravity;

        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0f && !Input.GetButton("Jump"))
        {
            rb.gravityScale = baseGravity * lowJumpGravityMultiplier;
        }

        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    public void SetExternalVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public void SetGravityEnabled(bool enabled)
    {
        rb.gravityScale = enabled ? baseGravity : 0f;
    }

    public void SetLocked(bool locked)
    {
        enabled = !locked;
        if (locked)
            rb.linearVelocity = Vector2.zero;
    }
}