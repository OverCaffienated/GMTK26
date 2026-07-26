using UnityEngine;
using System.Collections;

public class ShadowPlayback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Animator shadowAnim;
    [SerializeField] private GameObject glintParticle;
    [SerializeField] private GameObject attackHitboxObject;

    [Header("Audio")]
    [SerializeField] private AudioSource tickingAudioSource;
    [SerializeField] private float maxHearingDistance = 20f;
    [SerializeField] private float maxTickingVolume = 1f;

    [Header("Shadow Attack Audio")]
    [SerializeField] private AudioSource shadowAudioSource;
    [SerializeField] private AudioClip bodyAttackSound;
    [Range(0f, 1f)][SerializeField] private float bodyAttackVolume = 1f;
    [SerializeField] private AudioClip clockSummonSound;
    [Range(0f, 1f)][SerializeField] private float clockSummonVolume = 1f;

    [Header("Shadow Animations (Exact Names)")]
    [SerializeField] private string idleAnimName = "DeathPreppingAttack_0";
    [SerializeField] private string bodyAttackAnimName = "ReaperAttack";
    [SerializeField] private string clockAttackAnimName = "ReaperWeaponAttack";

    [Header("Shadow Movement Settings")]
    public float delaySeconds = 8f;
    [SerializeField] private float baseMoveSpeed = 2f;
    [SerializeField] private float attackTriggerDistance = 2.5f;
    [SerializeField] private float attackCooldown = 6f;
    [SerializeField] private float pushBackDistance = 15f;
    [SerializeField] private float hoverOffsetY = 1.0f;
    [SerializeField] private float hoverOffsetX = 1.0f;
    [SerializeField] private float flipInterval = 2f;

    [Header("Attack Timings (Zilean Bomb Style)")]
    [SerializeField] private float totalAttackDuration = 4.0f;
    [SerializeField] private float parryWindowDuration = 1.0f;

    private bool isAttacking = false;
    private bool canBeParriedNow = false;
    private float attackTimer = 0f;
    private float speedBoostTimer = 0f;
    private float flipTimer = 0f;
    private Vector3 defaultHitboxLocalPos;

    private void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController2D>();

        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            defaultHitboxLocalPos = attackHitboxObject.transform.localPosition;
        }

        if (shadowAnim != null && !string.IsNullOrEmpty(idleAnimName))
        {
            shadowAnim.Play(idleAnimName, -1, 0f);
        }

        Collider2D shadowCol = GetComponent<Collider2D>();
        Collider2D playerCol = player != null ? player.GetComponent<Collider2D>() : null;
        if (shadowCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(shadowCol, playerCol, true);
        }

        if (tickingAudioSource != null)
        {
            tickingAudioSource.loop = true;
            tickingAudioSource.volume = 0f;
            tickingAudioSource.Play();
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (tickingAudioSource != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            float vol = Mathf.Clamp01(1f - (dist / maxHearingDistance)) * maxTickingVolume;
            tickingAudioSource.volume = vol;
        }

        if (Time.timeSinceLevelLoad < delaySeconds) return;

        attackTimer += Time.deltaTime;
        flipTimer += Time.deltaTime;

        if (speedBoostTimer > 0f)
        {
            speedBoostTimer -= Time.deltaTime;
        }

        Vector2 shadowPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.y);

        float sideOffset = shadowPos2D.x >= playerPos2D.x ? hoverOffsetX : -hoverOffsetX;
        Vector2 targetPos2D = new Vector2(playerPos2D.x + sideOffset, playerPos2D.y + hoverOffsetY);

        if (canBeParriedNow)
        {
            PlayerCombatOrParry playerCombat = player.GetComponent<PlayerCombatOrParry>();
            if (playerCombat != null && playerCombat.IsParryActive)
            {
                SuccessfulParry();
                return;
            }
        }

        if (!isAttacking)
        {
            if (shadowAnim != null && !string.IsNullOrEmpty(idleAnimName))
            {
                shadowAnim.Play(idleAnimName, -1, 0f);
            }

            if (glintParticle != null && glintParticle.activeSelf) glintParticle.SetActive(false);
            if (attackHitboxObject != null && attackHitboxObject.activeSelf)
            {
                attackHitboxObject.SetActive(false);
                attackHitboxObject.transform.localPosition = defaultHitboxLocalPos;
            }

            float currentSpeed = baseMoveSpeed;
            if (speedBoostTimer > 0f)
            {
                currentSpeed *= 1.8f;
            }

            transform.position = Vector2.MoveTowards(shadowPos2D, targetPos2D, currentSpeed * Time.deltaTime);

            if (flipTimer >= flipInterval)
            {
                float xDiff = playerPos2D.x - transform.position.x;
                if (Mathf.Abs(xDiff) > 0.05f)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (int)Mathf.Sign(xDiff);
                    transform.localScale = scale;
                }
                flipTimer = 0f;
            }

            float distanceToTarget = Vector2.Distance(shadowPos2D, targetPos2D);
            if (distanceToTarget <= attackTriggerDistance && attackTimer >= attackCooldown)
            {
                StartCoroutine(ShadowAttackRoutine());
            }
        }
    }

    public void BoostSpeedTemporarily()
    {
        speedBoostTimer = 1.0f;
    }

    private IEnumerator ShadowAttackRoutine()
    {
        isAttacking = true;
        attackTimer = 0f;

        // 1. Play Body Attack Animation & Sound
        if (shadowAnim != null && !string.IsNullOrEmpty(bodyAttackAnimName))
        {
            shadowAnim.Play(bodyAttackAnimName, -1, 0f);
        }

        if (shadowAudioSource != null && bodyAttackSound != null)
        {
            shadowAudioSource.PlayOneShot(bodyAttackSound, bodyAttackVolume);
        }

        float bodyAnimLeadIn = 0.8f;
        float elapsed = 0f;

        while (elapsed < bodyAnimLeadIn)
        {
            elapsed += Time.deltaTime;
            Vector2 shadowPos2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.y);
            float sideOffset = shadowPos2D.x >= playerPos2D.x ? hoverOffsetX : -hoverOffsetX;
            Vector2 targetPos2D = new Vector2(playerPos2D.x + sideOffset, playerPos2D.y + hoverOffsetY);
            transform.position = Vector2.MoveTowards(shadowPos2D, targetPos2D, baseMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. Summon the Clock Hitbox & Play Clock Sound
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(true);

            if (shadowAudioSource != null && clockSummonSound != null)
            {
                shadowAudioSource.PlayOneShot(clockSummonSound, clockSummonVolume);
            }

            Animator clockAnim = attackHitboxObject.GetComponent<Animator>();
            if (clockAnim != null && !string.IsNullOrEmpty(clockAttackAnimName))
            {
                clockAnim.Play(clockAttackAnimName, -1, 0f);
            }
        }

        float clockActiveDuration = totalAttackDuration - bodyAnimLeadIn;
        float warningTime = clockActiveDuration - parryWindowDuration;
        if (warningTime < 0f) warningTime = 0f;

        elapsed = 0f;

        while (elapsed < warningTime)
        {
            elapsed += Time.deltaTime;

            Vector2 shadowPos2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.y);
            float sideOffset = shadowPos2D.x >= playerPos2D.x ? hoverOffsetX : -hoverOffsetX;
            Vector2 targetPos2D = new Vector2(playerPos2D.x + sideOffset, playerPos2D.y + hoverOffsetY);
            transform.position = Vector2.MoveTowards(shadowPos2D, targetPos2D, baseMoveSpeed * Time.deltaTime);

            if (attackHitboxObject != null)
            {
                attackHitboxObject.transform.position = player.transform.position + new Vector3(0, 0.5f, 0);
            }
            yield return null;
        }

        canBeParriedNow = true;
        if (glintParticle != null) glintParticle.SetActive(true);

        elapsed = 0f;
        while (elapsed < parryWindowDuration)
        {
            elapsed += Time.deltaTime;
            if (attackHitboxObject != null)
            {
                attackHitboxObject.transform.position = player.transform.position + new Vector3(0, 0.5f, 0);
            }
            yield return null;
        }

        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);

        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            attackHitboxObject.transform.localPosition = defaultHitboxLocalPos;
        }

        if (isAttacking)
        {
            PlayerCombatOrParry playerCombat = player.GetComponent<PlayerCombatOrParry>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(999);
            }
        }

        isAttacking = false;
        StartCoroutine(ParryFadeAndTeleportRoutine(delaySeconds));
    }

    private void SuccessfulParry()
    {
        canBeParriedNow = false;
        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            attackHitboxObject.transform.localPosition = defaultHitboxLocalPos;
        }

        PlayerCombatOrParry playerCombat = player.GetComponent<PlayerCombatOrParry>();
        if (playerCombat != null)
        {
            playerCombat.TriggerParryEffect();
            playerCombat.ResetParryCooldown();
        }

        StopAllCoroutines();
        StartCoroutine(ParryFadeAndTeleportRoutine(delaySeconds));
    }

    public void ApplyGuillotineRespawn()
    {
        isAttacking = false;
        canBeParriedNow = false;

        if (glintParticle != null) glintParticle.SetActive(false);
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
            attackHitboxObject.transform.localPosition = defaultHitboxLocalPos;
        }

        StopAllCoroutines();
        StartCoroutine(ParryFadeAndTeleportRoutine(10f));
    }

    private IEnumerator ParryFadeAndTeleportRoutine(float timeToBuy)
    {
        if (shadowAnim != null && !string.IsNullOrEmpty(idleAnimName))
        {
            shadowAnim.Play(idleAnimName, -1, 0f);
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;
        float fadeTime = 0.4f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            if (sr != null)
            {
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            yield return null;
        }

        Vector2 pushDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
        if (pushDir == Vector2.zero) pushDir = Vector2.left;

        float sideOffset = pushDir.x >= 0 ? hoverOffsetX : -hoverOffsetX;
        transform.position = player.transform.position + (Vector3)(pushDir * (baseMoveSpeed * timeToBuy)) + new Vector3(sideOffset, hoverOffsetY, 0);

        Vector2 backDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(backDir * pushBackDistance);

        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
            if (sr != null)
            {
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            yield return null;
        }

        if (sr != null)
        {
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        }

        isAttacking = false;
        attackTimer = 0f;
    }
}