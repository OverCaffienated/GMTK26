using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatOrParry playerCombat;
    [SerializeField] private ShadowPlayback reaperShadow;

    [Header("Parry UI")]
    [SerializeField] private Image parryIconButton;
    [SerializeField] private Sprite parryReadySprite;
    [SerializeField] private Sprite parryCooldownSprite;

    [Header("Reaper Clock UI")]
    [SerializeField] private RectTransform clockHandTransform;
    [SerializeField] private float maxDistanceForClock = 15f;
    [SerializeField] private float minRotationZ = 0f;
    [SerializeField] private float maxRotationZ = 360f;

    private void Update()
    {
        UpdateParryUI();
        UpdateClockUI();
    }

    private void UpdateParryUI()
    {
        if (parryIconButton == null || playerCombat == null) return;

        if (playerCombat.CanParry)
        {
            if (parryReadySprite != null) parryIconButton.sprite = parryReadySprite;
        }
        else
        {
            if (parryCooldownSprite != null) parryIconButton.sprite = parryCooldownSprite;
        }
    }

    private void UpdateClockUI()
    {
        if (clockHandTransform == null || reaperShadow == null) return;

        PlayerController2D player = Object.FindAnyObjectByType<PlayerController2D>();
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, reaperShadow.transform.position);
        float progress = Mathf.Clamp01(1f - (distance / maxDistanceForClock));

        float targetAngle = Mathf.Lerp(minRotationZ, maxRotationZ, progress);
        clockHandTransform.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }
}