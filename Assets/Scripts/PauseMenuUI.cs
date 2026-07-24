using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Settings Buttons")]
    [SerializeField] private Button settingsBackButton;

    [Header("Scene Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(OpenSettings);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.RemoveListener(OpenSettings);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(CloseSettings);
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void HandlePausePressed()
    {
        if (GameStateManager.Instance == null)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
        {
            OpenPauseMenu();
        }
        else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void OpenPauseMenu()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}