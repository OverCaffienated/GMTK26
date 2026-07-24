using UnityEngine;

public class PauseOverlay : MonoBehaviour
{
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private void Update()
    {
        if (GameStateManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI != null)
            {
                pauseMenuUI.HandlePausePressed();
            }
        }
    }
}