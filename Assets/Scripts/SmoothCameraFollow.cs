using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRb;
    [SerializeField] private PlayerSensors2D targetSensors;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Head Bob Settings")]
    [SerializeField] private float bobSpeed = 12f;
    [SerializeField] private float bobAmount = 0.15f;

    private Vector3 currentVelocity;
    private float bobTimer;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        if (targetRb != null && targetSensors != null)
        {
            if (targetSensors.IsGrounded && Mathf.Abs(targetRb.linearVelocity.x) > 0.5f)
            {
                bobTimer += Time.deltaTime * bobSpeed;
                float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
                targetPosition.y += bobOffset;
            }
            else
            {
                bobTimer = 0f;
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}