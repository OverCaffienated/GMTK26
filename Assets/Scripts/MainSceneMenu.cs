using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform introCameraPoint;
    [SerializeField] private Transform levelStartPoint;
    [SerializeField] private MonoBehaviour cameraFollowScript;
    [SerializeField] private float panDuration = 2f;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayPressed);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitPressed);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayPressed);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitPressed);
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        if (mainCamera != null && introCameraPoint != null)
        {
            mainCamera.transform.position = new Vector3(
                introCameraPoint.position.x,
                introCameraPoint.position.y,
                mainCamera.transform.position.z
            );
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.MainMenu);
    }

    public void OnPlayPressed()
    {
        StartCoroutine(BeginIntroPan());
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ShowMainMenuAgain()
    {
        StopAllCoroutines();

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && introCameraPoint != null)
        {
            mainCamera.transform.position = new Vector3(
                introCameraPoint.position.x,
                introCameraPoint.position.y,
                mainCamera.transform.position.z
            );
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.MainMenu);
    }

    private IEnumerator BeginIntroPan()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.IntroPan);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || levelStartPoint == null)
        {
            Debug.LogWarning("Pan failed because camera or levelStartPoint is missing.");
            yield break;
        }

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        Vector3 start = mainCamera.transform.position;
        Vector3 end = new Vector3(levelStartPoint.position.x, levelStartPoint.position.y, start.z);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        float t = 0f;
        while (t < panDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / panDuration);
            p = p * p * (3f - 2f * p);
            mainCamera.transform.position = Vector3.Lerp(start, end, p);
            yield return null;
        }

        mainCamera.transform.position = end;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
    }
}