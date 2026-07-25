using UnityEngine;
using UnityEngine.UI;

public class ShadowClockManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image clockImage;
    [SerializeField] private Transform player;
    [SerializeField] private Transform shadow;

    [Header("Clock Sprites (15 Min Increments)")]
    [SerializeField] private Sprite clock1115;
    [SerializeField] private Sprite clock1130;
    [SerializeField] private Sprite clock1145;
    [SerializeField] private Sprite clock1200;

    [Header("Distance Thresholds")]
    [SerializeField] private float distanceFor1130 = 20f;
    [SerializeField] private float distanceFor1145 = 12f;
    [SerializeField] private float distanceFor1200 = 5f;

    [Header("Shake Settings (At 12:00)")]
    [SerializeField] private float shakeIntensity = 3f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private void Start()
    {
        if (clockImage != null)
        {
            rectTransform = clockImage.GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (player == null || shadow == null || clockImage == null) return;

        float distance = Vector2.Distance(player.position, shadow.position);
        UpdateClockVisuals(distance);
    }

    private void UpdateClockVisuals(float distance)
    {
        if (distance <= distanceFor1200)
        {
            clockImage.sprite = clock1200;
            ApplyShake();
        }

        else if (distance <= distanceFor1145)
        {
            clockImage.sprite = clock1145;
            ResetPosition();
        }
        else if (distance <= distanceFor1130)
        {
            clockImage.sprite = clock1130;
            ResetPosition();
        }
        else
        {
            clockImage.sprite = clock1115;
            ResetPosition();
        }
    }

    private void ApplyShake()
    {
        if (rectTransform == null) return;
        Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
        rectTransform.anchoredPosition = originalPosition + shakeOffset;
    }

    private void ResetPosition()
    {
        if (rectTransform == null) return;

        if (rectTransform.anchoredPosition != originalPosition)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}