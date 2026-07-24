using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Enemy Identity")]
    [SerializeField] private bool isShadowEnemy = false;
    [SerializeField] private float shadowRewindSeconds = 4f;

    [Header("Combat Config")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float parryWindowDuration = 0.4f;
    [SerializeField] private GameObject yellowGlintVisual;
    [SerializeField] private GameObject attackHitboxObject;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    private Animator anim;
    private bool isAttacking = false;
    private bool canBeParriedNow = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (yellowGlintVisual != null) yellowGlintVisual.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        StartCoroutine(EnemyAttackLoop());
    }

    private IEnumerator EnemyAttackLoop()
    {
        while (true)
        {

            yield return new WaitForSeconds(attackCooldown);

            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) < 5f)
            {
                yield return StartCoroutine(ExecuteAttackSequence());
            }
        }
    }

    private IEnumerator ExecuteAttackSequence()
    {
        isAttacking = true;

        if (anim != null) anim.Play("AttackWindup");
        yield return new WaitForSeconds(0.5f);

        canBeParriedNow = true;
        if (yellowGlintVisual != null) yellowGlintVisual.SetActive(true);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(true);

        float timer = 0f;
        bool playerSuccessfullyParried = false;

        while (timer < parryWindowDuration)
        {
            PlayerCombatOrParry playerCombat = playerTransform != null ? playerTransform.GetComponent<PlayerCombatOrParry>() : null;

            if (playerCombat != null && playerCombat.IsParryActive)
            {
                Collider2D hit = Physics2D.OverlapPoint(playerTransform.position, LayerMask.GetMask("Player"));
                if (Vector2.Distance(transform.position, playerTransform.position) <= 2.5f)
                {
                    playerSuccessfullyParried = true;
                    break;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        canBeParriedNow = false;
        if (yellowGlintVisual != null) yellowGlintVisual.SetActive(false);
        if (attackHitboxObject != null) attackHitboxObject.SetActive(false);

        if (playerSuccessfullyParried)
        {
            HandleSuccessfulParry();
        }
        else
        {
            CheckPlayerHit();
        }

        isAttacking = false;
    }

    private void HandleSuccessfulParry()
    {
        Debug.Log(gameObject.name + " attack was successfully parried!");

        if (isShadowEnemy)
        {
            ShadowPlayback shadow = GetComponent<ShadowPlayback>();
            if (shadow != null)
            {
                shadow.delaySeconds += shadowRewindSeconds;
            }
            if (anim != null) anim.Play("Staggered");
        }
        else
        {
            if (anim != null) anim.Play("Blocked");
        }
    }

    private void CheckPlayerHit()
    {
        if (playerTransform == null) return;


        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= 2.0f)
        {
            PlayerCombatOrParry playerCombat = playerTransform.GetComponent<PlayerCombatOrParry>();
            if (playerCombat != null)
            {
                if (isShadowEnemy)
                {
                    playerCombat.TriggerPermanentDeath();
                }
                else
                {

                    playerCombat.TakeDamage(1);
                }
            }
        }
    }
}