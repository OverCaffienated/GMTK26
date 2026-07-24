using UnityEngine;
using System.Collections;

public class AdvancedEnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject glintParticle;
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private Transform visualRoot;

    [Header("Enemy Animations (Exact Names)")]
    [SerializeField] private string runAnimName = "EnemyRun";
    [SerializeField] private string idleAnimName = "EnemyIdle";
    [SerializeField] private string attackAnimName = "EnemyAttack";

    [Header("Health & Stats")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Detection & Patrol")]
    [SerializeField] private float aggroRadius = 7f;
    [SerializeField] private float patrolSpeed = 1.5f;
    private float patrolTimer = 0f;
    private int patrolDirection = 1;

    [Header("Movement & Strafing")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackTriggerDistance = 1.2f;
    [SerializeField] private bool invertFacing = false;
    [SerializeField] private float strafeTimerMin = 0.5f;
    [SerializeField] private float strafeTimerMax = 2.0f;

    [Header("Leaning")]
    [SerializeField] private float maxLeanAngle = 10f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Ledge Check")]
    [SerializeField] private bool enableLedgeCheck = true;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float ledgeCheckDistance = 1.0f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack Timings")]
    [SerializeField] private float totalAttackDuration = 1.0f;
    [SerializeField] private float parryWindowDuration = 0.25f;
    [SerializeField] private float minPauseBetweenAttacks = 0.5f;
    [SerializeField] private float maxPauseBetweenAttacks = 1.5f;

    private bool isAttacking = false;
    private bool isPaused = false;
    private bool canBeParriedNow = false;
    private float moveTimer = 0f;

    private int currentMoveDirection = 1;

    private void Start()
    {
        currentHealth = maxHealth;

        if (player == null) player = FindAnyObjectByType<PlayerController2D>();
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);

        PickNewMovementState();
    }

    private void Update()
    {
        if (player == null) return;

        if (canBeParriedNow)
        {
            PlayerCombatOrParry pc = player.GetComponent<PlayerCombatOrParry>();
            if (pc != null && pc.IsParryActive)
            {
                SuccessfulParry(pc);
                return;
            }
        }

        if (isAttacking || isPaused)
        {
            ApplyLeaning(0f);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance > aggroRadius)
        {
            HandlePatrolMovement();
        }
        else
        {
            UpdateFacing(player.transform.position.x - transform.position.x);

            if (distance > attackTriggerDistance)
            {
                HandleStrafingMovement();
            }
            else
            {
                ApplyLeaning(0f);
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private void UpdateFacing(float xDiff)
    {
        if (Mathf.Abs(xDiff) > 0.05f)
        {
            int sign = (int)Mathf.Sign(xDiff);
            if (invertFacing) sign *= -1;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * sign;
            transform.localScale = scale;
        }
    }

    private void HandlePatrolMovement()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0f)
        {
            patrolTimer = Random.Range(2f, 4f);
            float r = Random.value;
            if (r < 0.2f) patrolDirection = 0;
            else patrolDirection = Random.value > 0.5f ? 1 : -1;
        }

        bool isLedgeAhead = false;
        if (enableLedgeCheck && ledgeCheck != null && patrolDirection != 0)
        {
            Vector3 checkPos = transform.position + new Vector3(patrolDirection * 0.5f, 0, 0);
            isLedgeAhead = !Physics2D.Raycast(checkPos, Vector2.down, ledgeCheckDistance, groundLayer);
        }

        if (isLedgeAhead)
        {
            patrolDirection *= -1;
        }

        if (patrolDirection != 0)
        {
            UpdateFacing(patrolDirection);
            transform.position += new Vector3(patrolDirection * patrolSpeed * Time.deltaTime, 0, 0);
            if (anim != null && !string.IsNullOrEmpty(runAnimName)) anim.Play(runAnimName, -1, 0f);
            ApplyLeaning(patrolSpeed);
        }
        else
        {
            if (anim != null && !string.IsNullOrEmpty(idleAnimName)) anim.Play(idleAnimName, -1, 0f);
            ApplyLeaning(0f);
        }
    }

    private void HandleStrafingMovement()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            PickNewMovementState();
        }

        bool isLedgeAhead = false;
        if (enableLedgeCheck && ledgeCheck != null && currentMoveDirection != 0)
        {
            int facingPlayerDir = player.transform.position.x > transform.position.x ? 1 : -1;
            int actualMoveDir = facingPlayerDir * currentMoveDirection;

            Vector3 checkPos = transform.position + new Vector3(actualMoveDir * 0.5f, 0, 0);
            isLedgeAhead = !Physics2D.Raycast(checkPos, Vector2.down, ledgeCheckDistance, groundLayer);
        }

        float actualSpeedToMove = moveSpeed;
        if (isLedgeAhead)
        {
            currentMoveDirection = 0;
            actualSpeedToMove = 0f;
        }

        if (currentMoveDirection == 1)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), actualSpeedToMove * Time.deltaTime);
            if (anim != null && !string.IsNullOrEmpty(runAnimName)) anim.Play(runAnimName, -1, 0f);
            ApplyLeaning(actualSpeedToMove);
        }
        else if (currentMoveDirection == -1)
        {
            Vector2 awayTarget = new Vector2(transform.position.x + (transform.position.x > player.transform.position.x ? 1 : -1), transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, awayTarget, actualSpeedToMove * Time.deltaTime);
            if (anim != null && !string.IsNullOrEmpty(runAnimName)) anim.Play(runAnimName, -1, 0f);
            ApplyLeaning(-actualSpeedToMove);
        }
        else
        {
            if (anim != null && !string.IsNullOrEmpty(idleAnimName)) anim.Play(idleAnimName, -1, 0f);
            ApplyLeaning(0f);
        }
    }

    private void ApplyLeaning(float speedToLean)
    {
        if (visualRoot == null) return;

        float directionMult = transform.localScale.x > 0 ? 1f : -1f;
        if (invertFacing) directionMult *= -1f;

        float speedRatio = speedToLean / moveSpeed;
        float targetZRotation = speedRatio * -maxLeanAngle * directionMult;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        visualRoot.localRotation = Quaternion.Lerp(visualRoot.localRotation, targetRotation, Time.deltaTime * leanSpeed);
    }

    private void PickNewMovementState()
    {
        moveTimer = Random.Range(strafeTimerMin, strafeTimerMax);
        float chance = Random.value;
        if (chance > 0.5f) currentMoveDirection = 1;
        else if (chance > 0.25f) currentMoveDirection = -1;
        else currentMoveDirection = 0;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (anim != null && !string.IsNullOrEmpty(attackAnimName))
            anim.Play(attackAnimName, -1, 0f);

        float windup = totalAttackDuration - parryWindowDuration;
        yield return new WaitForSeconds(windup);

        canBeParriedNow = true;
        if (glintParticle != null) glintParticle.SetActive(true);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(parryWindowDuration);

        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);

        if (isAttacking)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist <= attackTriggerDistance + 0.5f)
            {
                PlayerCombatOrParry pc = player.GetComponent<PlayerCombatOrParry>();
                if (pc != null) pc.TakeDamage(1);
            }
        }

        isAttacking = false;
        isPaused = true;

        if (anim != null && !string.IsNullOrEmpty(runAnimName))
            anim.Play(runAnimName, -1, 0f);

        float pauseTime = Random.Range(minPauseBetweenAttacks, maxPauseBetweenAttacks);
        yield return new WaitForSeconds(pauseTime);

        isPaused = false;
    }

    private void SuccessfulParry(PlayerCombatOrParry pc)
    {
        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);

        pc.TriggerParryEffect();
        pc.ApplyParryPushback(transform.position);

        StopAllCoroutines();
        StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isAttacking = false;
        isPaused = true;

        Vector2 pushDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
        if (pushDir == Vector2.zero) pushDir = Vector2.right;
        transform.position += (Vector3)(pushDir * 1.5f);

        yield return new WaitForSeconds(2.0f);

        isPaused = false;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}