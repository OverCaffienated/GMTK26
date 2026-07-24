using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private PlayerCombatOrParry playerCombat;
    [SerializeField] private Image[] heartIcons;

    [Header("Shadow Clock UI")]
    [SerializeField] private Transform clockFace;
    [SerializeField] private Transform clockHand;
    [SerializeField] private float maxDistance = 18f;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Danger Vibration Settings")]
    [SerializeField] private float maxVibrationIntensity = 2f;

    private Transform playerTransform;
    private Transform shadowTransform;
    private Vector3 clockFaceOriginalPos;

    private void Start()
    {
        if (playerCombat != null)
            playerTransform = playerCombat.transform;

        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null)
            shadowTransform = shadow.transform;

        if (clockFace != null)
            clockFaceOriginalPos = clockFace.localPosition;
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateClockUI();
    }

    private void UpdateHealthUI()
    {
        if (playerCombat == null || heartIcons.Length == 0) return;

        int currentLives = playerCombat.CurrentLives;
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                heartIcons[i].enabled = i < currentLives;
            }
        }
    }

    private void UpdateClockUI()
    {
        if (playerTransform == null || shadowTransform == null || clockHand == null) return;

        float distance = Vector2.Distance(playerTransform.position, shadowTransform.position);
        distance = Mathf.Clamp(distance, 0f, maxDistance);

        float targetZAngle = (1f - (distance / maxDistance)) * 180f;

        float currentZAngle = clockHand.localEulerAngles.z;
        float smoothedAngle = Mathf.LerpAngle(currentZAngle, targetZAngle, smoothSpeed * Time.deltaTime);
        clockHand.localRotation = Quaternion.Euler(0f, 0f, smoothedAngle);

        if (clockFace != null)
        {
            float dangerFactor = 1f - (distance / maxDistance); 

            if (dangerFactor > 0.3f)
            {
                float offsetX = Random.Range(-1f, 1f) * maxVibrationIntensity * dangerFactor;
                float offsetY = Random.Range(-1f, 1f) * maxVibrationIntensity * dangerFactor;
                clockFace.localPosition = clockFaceOriginalPos + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                clockFace.localPosition = clockFaceOriginalPos;
            }
        }
    }
}