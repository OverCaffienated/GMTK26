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
    [SerializeField] private float parryPushbackDistance = 0.3f;
    [SerializeField] private GameObject parryEffectPrefab;
    private bool isParryActive = false;
    private float parryTimer = 0f;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackVisualObject;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Player Animations")]
    [SerializeField] private Animator anim;
    [SerializeField] private string attackAnimationName = "YourAttackAnimNameHere";
    [SerializeField] private string parryAnimationName = "YourParryAnimNameHere";

    private float nextAttackTime = 0f;

    public bool IsParryActive => isParryActive;
    public bool CanParry => !isParryActive;
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
    }

    private void HandleParryInput()
    {
        bool parryInput = false;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            parryInput = true;

        if (Keyboard.current != null && (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            parryInput = true;

        if (parryInput && !isParryActive)
        {
            StartCoroutine(ActivateParryWindow());
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
    }

    public void TriggerParryEffect()
    {
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
        Vector3 pushDir = (transform.position - attackerPosition).normalized;
        pushDir.y = 0;

        if (pushDir == Vector3.zero)
            pushDir = transform.localScale.x > 0 ? Vector3.left : Vector3.right;

        transform.position += pushDir.normalized * parryPushbackDistance;
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;

        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null) shadow.BoostSpeedTemporarily();

        if (currentLives <= 0) SceneManager.LoadScene(guillotineSceneName);
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