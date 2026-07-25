using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DynamicCameraZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSensors2D playerSensors;

    [Header("Zoom Settings")]
    [SerializeField] private float groundedSize = 2.0f;
    [SerializeField] private float aerialSize = 2.4f;
    [SerializeField] private float zoomSpeed = 5.0f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (playerSensors == null || cam == null) return;

        float targetSize = playerSensors.IsGrounded ? groundedSize : aerialSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }
}