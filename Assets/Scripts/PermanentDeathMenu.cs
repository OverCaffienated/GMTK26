using UnityEngine;
using UnityEngine.SceneManagement;

public class PermanentDeathMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "MainGame";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void RestartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}