using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerCombatOrParry : MonoBehaviour
{
    [Header("Health & Scene Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private string guillotineSceneName = "GuillotineScene";
    [SerializeField] private string permanentDeathSceneName = "PermanentDeathScene";
    private int currentLives;

    [Header("Parry Settings")]
    [SerializeField] private float parryDuration = 0.2f;
    [SerializeField] private float parryCooldown = 5.0f;
    [SerializeField] private float parryPushbackDistance = 0.3f;
    [SerializeField] private GameObject parryEffectPrefab;
    [SerializeField] private float parryShakeDuration = 0.2f;
    [SerializeField] private float parryShakeIntensity = 0.3f;
    private bool isParryActive = false;
    private float parryTimer = 0f;
    private float nextParryTime = 0f;
    private bool wasParryReady = false;

    [Header("Parry UI Display")]
    [SerializeField] private GameObject parryAvailableUI;
    [SerializeField] private GameObject parryUnavailableUI;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackVisualObject;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Player Audio")]
    [SerializeField] private AudioSource playerAudioSource;

    [SerializeField] private AudioClip parryReadySound;
    [Range(0f, 1f)][SerializeField] private float parryReadyVolume = 1f;

    [SerializeField] private AudioClip parryUsedSound;
    [Range(0f, 1f)][SerializeField] private float parryUsedVolume = 1f;

    [SerializeField] private AudioClip parrySuccessSound;
    [Range(0f, 1f)][SerializeField] private float parrySuccessVolume = 1f;

    [SerializeField] private AudioClip swordSwingSound;
    [Range(0f, 1f)][SerializeField] private float swordSwingVolume = 1f;

    [Header("Player Animations")]
    [SerializeField] private Animator anim;
    [SerializeField] private string attackAnimationName = "SwordSlashAnim";
    [SerializeField] private string parryAnimationName = "ParryClip";
    [SerializeField] private string idleAnimationName = "Idle";

    private float nextAttackTime = 0f;

    public bool IsParryActive => isParryActive;
    public bool CanParry => Time.time >= nextParryTime && !isParryActive;
    public int CurrentLives => currentLives;

    private void Start()
    {
        currentLives = maxLives;
        if (attackVisualObject != null) attackVisualObject.SetActive(false);
    }

    private void Update()
    {
        HandleParryInput();
        HandleAttackInput();
        UpdateParryUI();
    }

    private void UpdateParryUI()
    {
        bool currentParryReady = CanParry;

        if (currentParryReady)
        {
            if (parryAvailableUI != null) parryAvailableUI.SetActive(true);
            if (parryUnavailableUI != null) parryUnavailableUI.SetActive(false);

            if (!wasParryReady && playerAudioSource != null && parryReadySound != null)
            {
                playerAudioSource.PlayOneShot(parryReadySound, parryReadyVolume);
            }
        }
        else
        {
            if (parryAvailableUI != null) parryAvailableUI.SetActive(false);
            if (parryUnavailableUI != null) parryUnavailableUI.SetActive(true);
        }

        wasParryReady = currentParryReady;
    }

    private void HandleParryInput()
    {
        bool parryInput = false;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            parryInput = true;

        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            parryInput = true;

        if (parryInput && CanParry)
        {
            StartCoroutine(ActivateParryWindow());
            nextParryTime = Time.time + parryCooldown;

            if (playerAudioSource != null && parryUsedSound != null)
            {
                playerAudioSource.PlayOneShot(parryUsedSound, parryUsedVolume);
            }
        }
    }

    private void HandleAttackInput()
    {
        if (Time.time >= nextAttackTime)
        {
            bool attackInput = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                attackInput = true;

            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
                attackInput = true;

            if (attackInput && !isParryActive)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown;

                if (playerAudioSource != null && swordSwingSound != null)
                {
                    playerAudioSource.PlayOneShot(swordSwingSound, swordSwingVolume);
                }
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (anim != null && !string.IsNullOrEmpty(attackAnimationName))
        {
            anim.Play(attackAnimationName, -1, 0f);
        }

        if (attackVisualObject != null) attackVisualObject.SetActive(true);

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                AdvancedEnemyAI enemyAI = enemy.GetComponent<AdvancedEnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.TakeDamage(attackDamage);
                }
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        if (attackVisualObject != null) attackVisualObject.SetActive(false);

        if (anim != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            anim.Play(idleAnimationName);
        }
    }

    private IEnumerator ActivateParryWindow()
    {
        if (anim != null && !string.IsNullOrEmpty(parryAnimationName))
        {
            anim.Play(parryAnimationName, -1, 0f);
        }

        isParryActive = true;
        parryTimer = parryDuration;

        while (parryTimer > 0f)
        {
            parryTimer -= Time.deltaTime;
            yield return null;
        }

        isParryActive = false;

        if (anim != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            anim.Play(idleAnimationName);
        }
    }

    public void ResetParryCooldown()
    {
        nextParryTime = 0f;
    }

    public void TriggerParryEffect()
    {
        if (playerAudioSource != null && parrySuccessSound != null)
        {
            playerAudioSource.PlayOneShot(parrySuccessSound, parrySuccessVolume);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(parryShakeDuration, parryShakeIntensity);
        }

        if (parryEffectPrefab != null)
        {
            float randomXOffset = Random.Range(0.8f, 1.5f);
            float randomYOffset = Random.Range(-0.3f, 0.5f);
            int facingDir = transform.localScale.x >= 0 ? 1 : -1;

            Vector3 spawnPos = transform.position + new Vector3(randomXOffset * facingDir, randomYOffset, 0f);
            Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            GameObject fx = Instantiate(parryEffectPrefab, spawnPos, randomRot);
            Destroy(fx, 0.5f);
        }
    }

    public void ApplyParryPushback(Vector3 attackerPosition)
    {
        StringPushbackCheck();
    }

    private void StringPushbackCheck() { }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null) shadow.BoostSpeedTemporarily();

        if (currentLives <= 0)
        {
            if (GuillotineManager.Instance != null)
            {
                GuillotineManager.Instance.StartGuillotineEvent(this);
            }
            else
            {
                Debug.LogError("GuillotineManager is missing from the scene!");
            }
        }
    }

    public void ReviveWithLowHP()
    {
        currentLives = 1;
    }

    public void TriggerPermanentDeath()
    {
        SceneManager.LoadScene(permanentDeathSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}