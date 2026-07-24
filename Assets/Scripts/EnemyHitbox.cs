using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombatOrParry playerCombat = collision.GetComponent<PlayerCombatOrParry>();
            if (playerCombat != null)
            {
                if (!playerCombat.IsParryActive)
                {
                    playerCombat.TakeDamage(damageAmount);
                }
            }
        }
    }
}