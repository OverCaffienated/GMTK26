using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private PlayerCombatOrParry playerCombat;
    [SerializeField] private Image[] heartIcons;

    [Header("Shadow Clock UI")]
    [SerializeField] private Transform clockHand;
    [SerializeField] private float degreesPerMeter = 10f;
    [SerializeField] private float maxDistance = 18f;

    private Transform playerTransform;
    private Transform shadowTransform;

    private void Start()
    {
        if (playerCombat != null)
            playerTransform = playerCombat.transform;
        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null)
            shadowTransform = shadow.transform;
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

        float targetAngle = distance * degreesPerMeter;

        clockHand.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }
}