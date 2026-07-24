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

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public RaycastHit2D WallHit { get; private set; }
    public int WallSide { get; private set; }

    private int facing = 1;
    private Collider2D[] selfColliders;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

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
        if (groundCheck == null)
            return false;

        if (rb.linearVelocity.y > 0.05f)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            if (IsSelfCollider(hit))
                continue;

            if (hit.isTrigger)
                continue;

            return true;
        }

        return false;
    }

    private void CheckWall()
    {
        IsTouchingWall = false;
        WallHit = default;
        WallSide = 0;

        if (wallCheck == null)
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(wallCheck.position, Vector2.right * facing, wallDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null)
                continue;

            if (IsSelfCollider(hits[i].collider))
                continue;

            if (hits[i].collider.isTrigger)
                continue;

            WallHit = hits[i];
            IsTouchingWall = true;
            WallSide = facing;
            return;
        }
    }

    private bool IsSelfCollider(Collider2D col)
    {
        if (col == null || selfColliders == null)
            return false;

        for (int i = 0; i < selfColliders.Length; i++)
        {
            if (col == selfColliders[i])
                return true;
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
            Vector3 dir = Vector3.right * facing * wallDistance;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir);
            Gizmos.DrawWireSphere(wallCheck.position + dir, 0.03f);
        }
    }
}