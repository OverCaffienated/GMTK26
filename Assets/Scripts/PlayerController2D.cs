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

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 4f;
    public float BaseGravity => baseGravity;
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

    [Header("Debug")]
    [SerializeField] private bool logParry = true;

    public float MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public bool ParryPressedThisFrame { get; private set; }
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
        sensors.SetFacing(facing);
        sensors.Tick();

        UpdateFacing();
        UpdateTimers();
        UpdateWallSlideState();
        HandleJumpInput();
        HandleJumpCut();

        if (ParryPressedThisFrame && logParry)
            Debug.Log("Parry pressed");

        JumpPressedThisFrame = false;
        JumpReleasedThisFrame = false;
        ParryPressedThisFrame = false;
    }

    private void FixedUpdate()
    {
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
        if (sensors.IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;

        if (wallJumpLockCounter > 0f)
            wallJumpLockCounter -= Time.deltaTime;

        if (wallRegrabBlockCounter > 0f)
            wallRegrabBlockCounter -= Time.deltaTime;
    }

    private void UpdateWallSlideState()
    {
        bool pushingIntoWall =
            (MoveInput > 0.01f && facing == 1) ||
            (MoveInput < -0.01f && facing == -1);

        IsWallSliding =
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
        JumpHeld = true;
    }

    private bool IsPerfectEdgeJump(out float horizontalBonus, out float verticalBonus)
    {
        horizontalBonus = 0f;
        verticalBonus = 0f;

        RaycastHit2D groundHit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            perfectJumpGroundRayLength,
            sensors.groundLayer
        );

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
        Debug.Log("Move input = " + input);
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        JumpHeld = true;
        JumpPressedThisFrame = true;
        jumpBufferCounter = jumpBufferTime;
    }

    public void OnJumpRelease(InputValue value)
    {
        JumpHeld = false;
        JumpReleasedThisFrame = true;
    }

    public void OnParry(InputValue value)
    {
        if (!value.isPressed)
            return;

        ParryPressedThisFrame = true;
    }
}