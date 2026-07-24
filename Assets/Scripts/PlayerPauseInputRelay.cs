using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPauseInputRelay : MonoBehaviour
{
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private void Awake()
    {
        if (pauseMenuUI == null)
            pauseMenuUI = FindFirstObjectByType<PauseMenuUI>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool pausePressed =
            Keyboard.current.pKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame;

        if (!pausePressed)
            return;

        Debug.Log("Hardcoded pause key detected: P or Esc");

        if (pauseMenuUI != null)
            pauseMenuUI.HandlePausePressed();
        else
            Debug.LogWarning("PauseMenuUI reference is missing.");
    }
}