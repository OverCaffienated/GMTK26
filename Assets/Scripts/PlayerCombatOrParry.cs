using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerCombatOrParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject swordHitboxObject;
    [SerializeField] private Animator swordHitboxAnimator;
    [SerializeField] private GameObject parryParticle;
    [SerializeField] private GameObject deathScreenPanel;

    [Header("Stats")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    [Header("Parry Settings")]
    public bool IsParryActive { get; private set; }
    public float parryWindow = 0.2f;
    [SerializeField] private float parryCooldown = 1.5f;
    private float parryCooldownTimer = 0f;
    public bool CanParry => parryCooldownTimer <= 0f && !isDead;

    [SerializeField] private float shadowRewindAmount = 2f;
    private bool isDead = false;
    private bool awaitingReviveParry = false;

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private float attackCooldown = 0.4f;
    private bool isAttacking = false;
    private bool canAttack = true;

    private void Start()
    {
        currentLives = maxLives;

        if (swordHitboxObject != null) swordHitboxObject.SetActive(false);
        if (parryParticle != null) parryParticle.SetActive(false);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
    }

    private void Update()
    {
        if (parryCooldownTimer > 0f)
            parryCooldownTimer -= Time.deltaTime;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && canAttack && !isAttacking && !isDead)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        if (anim != null) anim.Play("Attack", -1, 0f);

        if (swordHitboxObject != null)
        {
            swordHitboxObject.SetActive(true);
            if (swordHitboxAnimator != null)
                swordHitboxAnimator.Play("SwordSlashAnim", -1, 0f);
        }

        yield return new WaitForSeconds(attackDuration);

        if (swordHitboxObject != null) swordHitboxObject.SetActive(false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown - attackDuration);
        canAttack = true;
    }

    public void OnParry(InputValue value)
    {
        if (value.isPressed && !isDead)
        {
            if (CanParry)
            {
                ActivateParry();
            }
        }
        else if (value.isPressed && awaitingReviveParry)
        {
            AttemptRevive();
        }
    }

    public void ActivateParry()
    {
        parryCooldownTimer = parryCooldown;
        StopAllCoroutines();
        StartCoroutine(ParryRoutine());
    }

    IEnumerator ParryRoutine()
    {
        IsParryActive = true;

        if (parryParticle != null)
        {
            parryParticle.SetActive(false);
            parryParticle.SetActive(true);
        }

        CheckGlintParryInteraction();

        yield return new WaitForSeconds(parryWindow);

        IsParryActive = false;
        if (parryParticle != null) parryParticle.SetActive(false);
    }

    private void CheckGlintParryInteraction()
    {
        ShadowPlayback shadow = Object.FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null)
        {
            ParryShadow(shadow);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentLives -= damage;
        Debug.Log("Player took damage! Lives remaining: " + currentLives);

        if (currentLives <= 0)
        {
            TriggerInstantDeath();
        }
    }

    public void TriggerInstantDeath()
    {
        if (isDead) return;
        isDead = true;

        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(true);

        if (anim != null)
            anim.Play("Death");

        StartCoroutine(ReviveWindowRoutine());
    }

    private IEnumerator ReviveWindowRoutine()
    {
        awaitingReviveParry = true;
        float reviveTimer = 2.0f;

        yield return new WaitForSeconds(reviveTimer);

        if (awaitingReviveParry)
        {
            awaitingReviveParry = false;
        }
    }

    private void AttemptRevive()
    {
        awaitingReviveParry = false;
        isDead = false;
        currentLives = 1;

        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);

        if (anim != null)
            anim.Play("Idle");
    }

    public void ParryShadow(ShadowPlayback shadow)
    {
        shadow.delaySeconds += shadowRewindAmount;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}