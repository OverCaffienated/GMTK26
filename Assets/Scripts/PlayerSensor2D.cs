using UnityEngine;

public class PlayerSensors2D : MonoBehaviour
{
    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform ledgeCheck;

    [Header("Ground")]
    [SerializeField] private float groundRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Wall")]
    [SerializeField] private float wallDistance = 0.25f;
    public LayerMask wallLayer;

    [Header("Ledge")]
    [SerializeField] private Vector2 ledgeBoxSize = new Vector2(0.4f, 0.8f);
    public LayerMask ledgeLayer;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public RaycastHit2D WallHit { get; private set; }
    public Collider2D LedgeCandidate { get; private set; }

    private int facing = 1;

    public void SetFacing(int dir)
    {
        facing = dir == 0 ? 1 : dir;
    }

    public void Tick()
    {
        if (groundCheck != null)
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        else
            IsGrounded = false;

        if (wallCheck != null)
            WallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facing, wallDistance, wallLayer);
        else
            WallHit = default;

        IsTouchingWall = WallHit.collider != null;

        if (ledgeCheck != null)
            LedgeCandidate = Physics2D.OverlapBox(ledgeCheck.position, ledgeBoxSize, 0f, ledgeLayer);
        else
            LedgeCandidate = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * facing * wallDistance
            );
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ledgeCheck.position, ledgeBoxSize);
        }
    }
}