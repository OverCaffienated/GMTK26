using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Airborne;

    public void SetState(PlayerState newState)
    {
        CurrentState = newState;
    }

    public bool IsBusyState()
    {
        return CurrentState == PlayerState.LedgeHang ||
               CurrentState == PlayerState.LedgeClimb ||
               CurrentState == PlayerState.Dead;
    }
}