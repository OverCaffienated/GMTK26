using UnityEngine;

public class ShadowContact : MonoBehaviour
{
    public PlayerCombatOrParry parry;
    public PlayerStateMachine stateMachine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Shadow")) return;

        if (parry.IsParryActive)
            parry.ParryShadow(other.GetComponent<ShadowPlayback>());
        else
            stateMachine.SetState(PlayerState.Dead);
    }
}