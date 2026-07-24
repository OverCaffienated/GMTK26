using UnityEngine;

public class PauseOverlay : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private void Start()
    {
        pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (GameStateManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
                Pause();
            else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Paused)
                Resume();
        }
    }

    public void Pause()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        pausePanel.SetActive(false);
    }
}