using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombatOrParry combat = collision.GetComponent<PlayerCombatOrParry>();
            if (combat != null)
            {
                combat.TriggerInstantDeath();
            }
        }
    }
}