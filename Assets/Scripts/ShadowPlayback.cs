using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShadowPlayback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Animator shadowAnim;
    [SerializeField] private GameObject glintParticle;

    [Header("Shadow Settings")]
    public float delaySeconds = 2f;
    [SerializeField] private float attackTriggerDistance = 2.5f;
    [SerializeField] private float attackCooldown = 4f;

    private struct PlayerState
    {
        public Vector3 position;
        public int facing;
        public float time;
    }

    private Queue<PlayerState> history = new Queue<PlayerState>();
    private bool isAttacking = false;
    private float attackTimer = 0f;

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController2D>();
        if (glintParticle != null) glintParticle.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        attackTimer += Time.deltaTime;

        history.Enqueue(new PlayerState
        {
            position = player.transform.position,
            facing = player.Facing,
            time = Time.time
        });

        while (history.Count > 0 && Time.time - history.Peek().time > delaySeconds)
        {
            PlayerState state = history.Dequeue();
            if (!isAttacking)
            {
                transform.position = state.position;

                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * state.facing;
                transform.localScale = scale;
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackTriggerDistance && attackTimer >= attackCooldown && !isAttacking)
        {
            StartCoroutine(ShadowAttackRoutine());
        }
    }

    private IEnumerator ShadowAttackRoutine()
    {
        isAttacking = true;
        attackTimer = 0f;

        if (shadowAnim != null)
        {
            shadowAnim.Play("ReaperAttack", -1, 0f);
        }

        yield return new WaitForSeconds(0.4f);

        if (glintParticle != null)
        {
            glintParticle.SetActive(true);
        }

        yield return new WaitForSeconds(0.25f);

        if (glintParticle != null)
        {
            glintParticle.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }
}