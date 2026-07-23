using UnityEngine;

public class PlayerSensors2D : MonoBehaviour
{
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;

    public float groundRadius = 0.12f;
    public float wallDistance = 0.25f;
    public Vector2 ledgeBoxSize = new Vector2(0.4f, 0.8f);

    public LayerMask groundLayer;
    public LayerMask wallLayer;
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

        if (wallCheck != null)
            WallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facing, wallDistance, wallLayer);

        IsTouchingWall = WallHit.collider != null;

        if (ledgeCheck != null)
            LedgeCandidate = Physics2D.OverlapBox(ledgeCheck.position, ledgeBoxSize, 0f, ledgeLayer);
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
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * facing * wallDistance);
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ledgeCheck.position, ledgeBoxSize);
        }
    }
}