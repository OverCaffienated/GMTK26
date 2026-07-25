using UnityEngine;

public class PlayerSensors2D : MonoBehaviour
{
    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Ground")]
    [SerializeField] private float groundRadius = 0.1f;

    [Header("Wall")]
    [SerializeField] private float wallDistance = 0.2f;
    [SerializeField] private Vector2 wallBoxSize = new Vector2(0.1f, 0.5f);

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public RaycastHit2D WallHit { get; private set; }
    public int WallSide { get; private set; }

    private int facing = 1;
    private Collider2D[] selfColliders;

    private readonly Collider2D[] groundHitsBuffer = new Collider2D[8];
    private readonly RaycastHit2D[] wallHitsBuffer = new RaycastHit2D[8];

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        selfColliders = GetComponentsInChildren<Collider2D>();
    }

    public void SetFacing(int dir)
    {
        facing = dir == 0 ? 1 : dir;
    }

    public void Tick()
    {
        IsGrounded = CheckGrounded();
        CheckWall();
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        if (rb.linearVelocity.y > 0.05f) return false;

        int count = Physics2D.OverlapCircleNonAlloc(groundCheck.position, groundRadius, groundHitsBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = groundHitsBuffer[i];
            if (hit == null) continue;
            if (IsSelfCollider(hit)) continue;
            if (hit.isTrigger) continue;
            return true;
        }
        return false;
    }

    private void CheckWall()
    {
        IsTouchingWall = false;
        WallHit = default;
        WallSide = 0;

        if (wallCheck == null) return;

        if (TryFindWall(1)) return;
        if (TryFindWall(-1)) return;
    }

    private bool TryFindWall(int side)
    {
        int count = Physics2D.BoxCastNonAlloc(wallCheck.position, wallBoxSize, 0f, Vector2.right * side, wallHitsBuffer, wallDistance);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = wallHitsBuffer[i].collider;
            if (col == null) continue;
            if (IsSelfCollider(col)) continue;
            if (col.isTrigger) continue;

            WallHit = wallHitsBuffer[i];
            IsTouchingWall = true;
            WallSide = side;
            return true;
        }
        return false;
    }

    private bool IsSelfCollider(Collider2D col)
    {
        if (col == null || selfColliders == null) return false;
        for (int i = 0; i < selfColliders.Length; i++)
        {
            if (col == selfColliders[i]) return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = IsTouchingWall ? Color.green : Color.cyan;
            Vector3 rightDir = Vector3.right * wallDistance;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + rightDir);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position - rightDir);

            if (IsTouchingWall)
            {
                Gizmos.DrawWireCube(wallCheck.position + Vector3.right * WallSide * wallDistance, wallBoxSize);
            }
        }
    }
}