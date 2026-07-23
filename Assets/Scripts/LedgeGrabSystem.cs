using UnityEngine;
using System.Collections;

public class LedgeGrabSystem : MonoBehaviour
{
    public PlayerController2D controller;
    public PlayerStateMachine stateMachine;
    public Rigidbody2D rb;
    public PlayerSensors2D sensors;

    [Header("Grab")]
    public Vector2 grabBoxSize = new Vector2(0.5f, 1.0f);
    public Vector2 grabBoxOffset = new Vector2(0.45f, 0.2f);
    public float upperHalfFastClimbTime = 0.18f;
    public float lowerHalfClimbTime = 0.35f;

    [Header("Snap Positions")]
    public Vector2 hangOffset = new Vector2(0.35f, -0.2f);
    public Vector2 standOffset = new Vector2(0.35f, 0.9f);

    private bool canGrab = true;
    private Collider2D currentLedge;
    private Vector2 ledgePoint;

    void Update()
    {
        if (stateMachine.CurrentState == PlayerState.LedgeClimb || !canGrab) return;
        if (TryFindLedge(out Collider2D ledge, out Vector2 point, out bool upperHalf))
        {
            BeginLedgeHang(ledge, point, upperHalf);
        }
    }

    bool TryFindLedge(out Collider2D ledge, out Vector2 point, out bool upperHalf)
    {
        ledge = null;
        point = Vector2.zero;
        upperHalf = false;

        if (controller == null || sensors.IsGrounded || rb.linearVelocity.y > 0) return false;

        int facing = controller.transform.localScale.x >= 0 ? 1 : -1;
        Vector2 center = (Vector2)transform.position + new Vector2(grabBoxOffset.x * facing, grabBoxOffset.y);

        Collider2D hit = Physics2D.OverlapBox(center, grabBoxSize, 0f, sensors.ledgeLayer);
        if (!hit) return false;

        Bounds b = hit.bounds;
        float midY = (b.min.y + b.max.y) * 0.5f;
        upperHalf = transform.position.y >= midY;

        float x = facing > 0 ? b.min.x : b.max.x;
        float y = b.max.y;
        point = new Vector2(x, y);

        ledge = hit;
        return true;
    }

    void BeginLedgeHang(Collider2D ledge, Vector2 point, bool upperHalf)
    {
        currentLedge = ledge;
        ledgePoint = point;

        stateMachine.SetState(PlayerState.LedgeHang);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        int facing = controller.transform.localScale.x >= 0 ? 1 : -1;
        transform.position = point + new Vector2(-hangOffset.x * facing, hangOffset.y);

        StartCoroutine(ClimbRoutine(upperHalf ? upperHalfFastClimbTime : lowerHalfClimbTime));
    }

    IEnumerator ClimbRoutine(float duration)
    {
        stateMachine.SetState(PlayerState.LedgeClimb);

        Vector3 start = transform.position;
        int facing = controller.transform.localScale.x >= 0 ? 1 : -1;
        Vector3 end = ledgePoint + new Vector2(standOffset.x * facing, standOffset.y);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }

        rb.gravityScale = controller.BaseGravity;
        stateMachine.SetState(PlayerState.Airborne);

        canGrab = false;
        yield return new WaitForSeconds(0.15f);
        canGrab = true;
    }
}