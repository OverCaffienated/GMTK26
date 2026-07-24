using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public enum SettingsReturnTarget
    {
        None,
        MainMenu,
        PauseMenu
    }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button mainMenuSettingsButton;

    [Header("Settings Buttons")]
    [SerializeField] private Button settingsBackButton;

    [Header("Refs")]
    [SerializeField] private MainSceneMenu mainSceneMenu;

    private SettingsReturnTarget settingsReturnTarget = SettingsReturnTarget.None;

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(OpenSettingsFromPause);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (mainMenuSettingsButton != null)
            mainMenuSettingsButton.onClick.AddListener(OpenSettingsFromMainMenu);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.RemoveListener(OpenSettingsFromPause);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        if (mainMenuSettingsButton != null)
            mainMenuSettingsButton.onClick.RemoveListener(OpenSettingsFromMainMenu);

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

        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.MainMenu)
            return;

        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.IntroPan)
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
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        settingsReturnTarget = SettingsReturnTarget.None;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
    }

    public void OpenSettingsFromPause()
    {
        settingsReturnTarget = SettingsReturnTarget.PauseMenu;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Opened settings from pause");
    }

    public void OpenSettingsFromMainMenu()
    {
        settingsReturnTarget = SettingsReturnTarget.MainMenu;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Opened settings from main menu");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsReturnTarget == SettingsReturnTarget.PauseMenu)
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else if (settingsReturnTarget == SettingsReturnTarget.MainMenu)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        settingsReturnTarget = SettingsReturnTarget.None;
    }

    public void ReturnToMainMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        settingsReturnTarget = SettingsReturnTarget.None;

        if (mainSceneMenu != null)
            mainSceneMenu.ShowMainMenuAgain();
    }
}