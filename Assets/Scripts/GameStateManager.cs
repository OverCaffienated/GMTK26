using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        IntroPan,
        Playing,
        Paused
    }

    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public bool GameplayLocked =>
        CurrentState == GameState.MainMenu ||
        CurrentState == GameState.IntroPan ||
        CurrentState == GameState.Paused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Game State -> " + newState);
    }
}