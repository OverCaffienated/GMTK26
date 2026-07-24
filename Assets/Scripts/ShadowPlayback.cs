using UnityEngine;
using System.Collections;

public class ShadowPlayback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Animator shadowAnim;
    [SerializeField] private GameObject glintParticle;
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private string permanentDeathSceneName = "PermanentDeathScene";

    [Header("Shadow Settings")]
    public float delaySeconds = 8f;
    [SerializeField] private float baseMoveSpeed = 4f;
    [SerializeField] private float attackTriggerDistance = 0.5f;
    [SerializeField] private float attackCooldown = 4f;
    [SerializeField] private float pushBackDistance = 3f;
    [SerializeField] private float hoverOffsetY = 2.0f;
    [SerializeField] private float hoverOffsetX = 2.0f;
    [SerializeField] private float flipInterval = 2.0f;

    [Header("Attack Timing Config")]
    [SerializeField] private float parryWindowDuration = 0.25f;

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

        Collider2D shadowCol = GetComponent<Collider2D>();
        Collider2D playerCol = player != null ? player.GetComponent<Collider2D>() : null;
        if (shadowCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(shadowCol, playerCol, true);
        }
    }

    private void Update()
    {
        if (player == null) return;

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

        if (!isAttacking)
        {
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
        }

        float distanceToTarget = Vector2.Distance(shadowPos2D, targetPos2D);
        if (distanceToTarget <= attackTriggerDistance && attackTimer >= attackCooldown && !isAttacking)
        {
            StartCoroutine(ShadowAttackRoutine());
        }

        if (canBeParriedNow)
        {
            PlayerCombatOrParry playerCombat = player.GetComponent<PlayerCombatOrParry>();
            if (playerCombat != null && playerCombat.IsParryActive)
            {
                SuccessfulParry();
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

        float xDiff = player.transform.position.x - transform.position.x;
        if (Mathf.Abs(xDiff) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (int)Mathf.Sign(xDiff);
            transform.localScale = scale;
        }
        flipTimer = 0f;

        if (shadowAnim != null)
        {
            shadowAnim.Play("clockreaperbloodattack", -1, 0f);
        }

        if (attackHitboxObject != null) attackHitboxObject.SetActive(true);

        float totalAttackTime = 1.367f;
        float windupTime = totalAttackTime - parryWindowDuration;
        float elapsed = 0f;

        while (elapsed < windupTime)
        {
            elapsed += Time.deltaTime;
            if (attackHitboxObject != null)
            {
                attackHitboxObject.transform.position = player.transform.position;
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
                attackHitboxObject.transform.position = player.transform.position;
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
            TriggerPermanentDeath();
        }

        isAttacking = false;

        if (shadowAnim != null)
        {
            shadowAnim.Play("DeathPreppingAttack_0", -1, 0f);
        }
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
        // ANIMATION RESET: Instantly reverts to the default pose while it fades out and teleports
        if (shadowAnim != null)
        {
            shadowAnim.Play("DeathPreppingAttack_0", -1, 0f);
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

    private void TriggerPermanentDeath()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(permanentDeathSceneName);
    }
}