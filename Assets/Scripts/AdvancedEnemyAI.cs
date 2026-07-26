using UnityEngine;
using System.Collections;

public class AdvancedEnemyAI : MonoBehaviour
{
    public enum EnemyType { Standard, Tank, Expert }

    [Header("Enemy Archetype")]
    [SerializeField] private EnemyType archetype = EnemyType.Standard;

    [Header("References")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject glintParticle;
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private Transform visualRoot;

    [Header("Enemy Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip aggroGruntSound;
    [Range(0f, 1f)][SerializeField] private float gruntVolume = 1f;

    [Header("Damage Audio")]
    [SerializeField] private AudioClip damageTakenSound;
    [Range(0f, 1f)][SerializeField] private float damageTakenVolume = 1f;

    [Header("Block/Parry Audio")]
    [SerializeField] private AudioClip blockSound;
    [Range(0f, 1f)][SerializeField] private float blockVolume = 1f;

    [Header("Enemy Animations (Exact Names)")]
    [SerializeField] private string runAnimName = "EnemyRun";
    [SerializeField] private string idleAnimName = "EnemyIdle";
    [SerializeField] private string attackAnimName = "EnemyAttack";

    [Header("Health & Stats")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Death Settings")]
    [SerializeField] private float sinkSpeed = 1.5f;
    [SerializeField] private float sinkDuration = 2.0f;

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

    [Header("Attack Timings & Variance")]
    [SerializeField] private float totalAttackDuration = 1.0f;
    [SerializeField] private float parryWindowDuration = 0.25f;
    [SerializeField] private float minPauseBetweenAttacks = 0.5f;
    [SerializeField] private float maxPauseBetweenAttacks = 1.5f;
    [SerializeField] private float minAttackSpeedBoost = 0.0f;
    [SerializeField] private float maxAttackSpeedBoost = 0.15f;

    [Header("Tank / Expert Settings")]
    [SerializeField] private string stunAnimName = "EnemyStun";
    [SerializeField] private float stunDuration = 2.0f;
    [SerializeField] private int requiredParriesToStun = 2;
    [SerializeField] private float expertParryCooldown = 2.5f;
    [SerializeField] private string expertParryAnimName = "EnemyParry";
    [SerializeField] private GameObject expertGlintParticle;

    private bool isAttacking = false;
    private bool isPaused = false;
    private bool canBeParriedNow = false;
    private float moveTimer = 0f;
    private bool wasAggroLastFrame = false;
    private int currentMoveDirection = 1;
    private Rigidbody2D rb;

    private bool isStunned = false;
    private bool isDead = false;
    private int currentParriesReceived = 0;
    private float expertParryTimer = 0f;
    private float parryDebounceTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (player == null) player = FindAnyObjectByType<PlayerController2D>();
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);
        if (expertGlintParticle != null) expertGlintParticle.SetActive(false);

        PickNewMovementState();
    }

    private void Update()
    {
        if (player == null || isDead) return;

        if (expertParryTimer > 0f) expertParryTimer -= Time.deltaTime;
        if (parryDebounceTimer > 0f) parryDebounceTimer -= Time.deltaTime;

        if (archetype == EnemyType.Expert && !isStunned && !isAttacking && !isPaused)
        {
            PlayerCombatOrParry pc = player.GetComponent<PlayerCombatOrParry>();
            if (pc != null && pc.IsAttacking && expertParryTimer <= 0f)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= attackTriggerDistance + 1f)
                {
                    StartCoroutine(ExpertParryPlayerRoutine(pc));
                    return;
                }
            }
        }

        if (canBeParriedNow && parryDebounceTimer <= 0f)
        {
            PlayerCombatOrParry pc = player.GetComponent<PlayerCombatOrParry>();
            if (pc != null && pc.IsParryActive)
            {
                parryDebounceTimer = 0.2f;
                SuccessfulParry(pc);
                return;
            }
        }

        if (isAttacking || isPaused || isStunned)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            ApplyLeaning(0f);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);
        bool isAggro = distance <= aggroRadius;

        if (!wasAggroLastFrame && isAggro)
        {
            if (audioSource != null && aggroGruntSound != null)
            {
                audioSource.PlayOneShot(aggroGruntSound, gruntVolume);
            }
        }
        wasAggroLastFrame = isAggro;

        if (!isAggro)
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
                if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

    private void PlayAnim(string animName, bool forceRestart = false)
    {
        if (anim == null || string.IsNullOrEmpty(animName)) return;
        if (forceRestart)
        {
            anim.Play(animName, -1, 0f);
        }
        else
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName(animName))
            {
                anim.Play(animName);
            }
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
            Vector3 checkPos = ledgeCheck.position + new Vector3(patrolDirection * 0.5f, 0, 0);
            isLedgeAhead = !Physics2D.Raycast(checkPos, Vector2.down, ledgeCheckDistance, groundLayer);
        }

        if (isLedgeAhead)
        {
            patrolDirection *= -1;
        }

        if (patrolDirection != 0)
        {
            UpdateFacing(patrolDirection);
            if (rb != null) rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);
            else transform.position += new Vector3(patrolDirection * patrolSpeed * Time.deltaTime, 0, 0);

            PlayAnim(runAnimName);
            ApplyLeaning(patrolSpeed);
        }
        else
        {
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            PlayAnim(idleAnimName);
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

        int facingPlayerDir = player.transform.position.x > transform.position.x ? 1 : -1;
        int actualMoveDir = facingPlayerDir * currentMoveDirection;

        bool isLedgeAhead = false;
        if (enableLedgeCheck && ledgeCheck != null && actualMoveDir != 0)
        {
            Vector3 checkPos = ledgeCheck.position + new Vector3(actualMoveDir * 0.5f, 0, 0);
            isLedgeAhead = !Physics2D.Raycast(checkPos, Vector2.down, ledgeCheckDistance, groundLayer);
        }

        if (isLedgeAhead)
        {
            currentMoveDirection = 0;
            actualMoveDir = 0;
        }

        if (actualMoveDir != 0)
        {
            if (rb != null) rb.linearVelocity = new Vector2(actualMoveDir * moveSpeed, rb.linearVelocity.y);
            else transform.position += new Vector3(actualMoveDir * moveSpeed * Time.deltaTime, 0, 0);

            PlayAnim(runAnimName);
            ApplyLeaning(actualMoveDir == facingPlayerDir ? moveSpeed : -moveSpeed);
        }
        else
        {
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            PlayAnim(idleAnimName);
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

    private IEnumerator ExpertWeaponRoutine(float delay, float speedMod)
    {
        yield return new WaitForSeconds(delay);

        if (isAttacking && attackHitboxObject != null && !isStunned && !isDead)
        {
            attackHitboxObject.SetActive(true);
            Animator weaponAnim = attackHitboxObject.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.speed = speedMod;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        float speedMod = 1f + Random.Range(minAttackSpeedBoost, maxAttackSpeedBoost);
        if (anim != null) anim.speed = speedMod;

        PlayAnim(attackAnimName, true);

        if (archetype == EnemyType.Expert)
        {
            float expertDelay = 0.2f / speedMod;
            StartCoroutine(ExpertWeaponRoutine(expertDelay, speedMod));
        }

        float currentTotal = totalAttackDuration / speedMod;
        float currentParry = parryWindowDuration / speedMod;
        float windup = currentTotal - currentParry;

        yield return new WaitForSeconds(windup);

        canBeParriedNow = true;
        if (glintParticle != null) glintParticle.SetActive(true);

        if (archetype != EnemyType.Expert && attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(true);
            Animator weaponAnim = attackHitboxObject.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.speed = speedMod;
        }

        yield return new WaitForSeconds(currentParry);

        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            Animator weaponAnim = attackHitboxObject.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.speed = 1f;
        }

        if (anim != null) anim.speed = 1f;

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

        PlayAnim(runAnimName);

        float pauseTime = Random.Range(minPauseBetweenAttacks, maxPauseBetweenAttacks);
        yield return new WaitForSeconds(pauseTime);

        isPaused = false;
    }

    private IEnumerator ExpertParryPlayerRoutine(PlayerCombatOrParry pc)
    {
        isPaused = true;
        expertParryTimer = expertParryCooldown;

        if (audioSource != null && blockSound != null)
        {
            audioSource.PlayOneShot(blockSound, blockVolume);
        }

        PlayAnim(expertParryAnimName, true);
        if (expertGlintParticle != null) expertGlintParticle.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        if (pc != null)
        {
            pc.ApplyParryPushback(transform.position);
        }

        if (expertGlintParticle != null) expertGlintParticle.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        isPaused = false;
    }

    private void SuccessfulParry(PlayerCombatOrParry pc)
    {
        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            Animator weaponAnim = attackHitboxObject.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.speed = 1f;
        }

        if (anim != null) anim.speed = 1f;

        pc.TriggerParryEffect();
        pc.ApplyParryPushback(transform.position);
        pc.ResetParryCooldown();

        StopAllCoroutines();

        if (archetype == EnemyType.Tank || archetype == EnemyType.Expert)
        {
            currentParriesReceived++;
            Debug.Log("Parry Successful! Current Count: " + currentParriesReceived + " / Required: " + requiredParriesToStun);

            if (currentParriesReceived >= requiredParriesToStun)
            {
                currentParriesReceived = 0;
                StartCoroutine(StunRoutine(stunDuration));
            }
            else
            {
                StartCoroutine(ParryRecoveryRoutine());
            }
        }
        else
        {
            StartCoroutine(StunRoutine(2.0f));
        }
    }

    private IEnumerator ParryRecoveryRoutine()
    {
        isAttacking = false;
        isPaused = true;

        if (rb != null)
        {
            float pushDir = transform.position.x > player.transform.position.x ? 1f : -1f;
            float force = archetype == EnemyType.Tank ? 2f : 5f;
            rb.linearVelocity = new Vector2(pushDir * force, rb.linearVelocity.y);
        }

        PlayAnim(idleAnimName, true);

        yield return new WaitForSeconds(0.6f);

        isPaused = false;
    }

    private IEnumerator StunRoutine(float duration)
    {
        isAttacking = false;
        isPaused = true;
        isStunned = true;

        PlayAnim(stunAnimName, true);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        isPaused = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        if (archetype == EnemyType.Tank || archetype == EnemyType.Expert)
        {
            if (!isStunned)
            {
                if (audioSource != null && blockSound != null)
                {
                    audioSource.PlayOneShot(blockSound, blockVolume);
                }
                return;
            }
        }

        if (audioSource != null && damageTakenSound != null)
        {
            audioSource.PlayOneShot(damageTakenSound, damageTakenVolume);
        }

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
        }

        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);
        if (expertGlintParticle != null) expertGlintParticle.SetActive(false);

        if (anim != null) anim.speed = 0f;

        float timer = 0f;
        while (timer < sinkDuration)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}