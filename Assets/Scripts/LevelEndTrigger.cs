using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string finalCutsceneSceneName = "EndCutscene";

    [Header("Camera & Zoom")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomedCamSize = 1.2f;
    [SerializeField] private float zoomDuration = 2.0f;

    [Header("Player Sprite Progression")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Sprite firstSprite;
    [SerializeField] private Sprite secondSprite;
    [SerializeField] private float delayBeforeSecondSprite = 2.0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(EndGameRoutine(collision));
        }
    }

    private IEnumerator EndGameRoutine(Collider2D collision)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameplayLocked = true;
        }

        PlayerController2D playerMovement = collision.GetComponent<PlayerController2D>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if (playerSpriteRenderer != null && firstSprite != null)
        {
            playerSpriteRenderer.sprite = firstSprite;
        }

        StartCoroutine(MuteWorldAudioRoutine(zoomDuration));

        float startCamSize = mainCamera != null ? mainCamera.orthographicSize : 2f;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / zoomDuration;

            if (mainCamera != null)
            {
                mainCamera.orthographicSize = Mathf.Lerp(startCamSize, zoomedCamSize, percent);
            }
            yield return null;
        }

        if (mainCamera != null) mainCamera.orthographicSize = zoomedCamSize;

        yield return new WaitForSeconds(delayBeforeSecondSprite);

        if (playerSpriteRenderer != null && secondSprite != null)
        {
            playerSpriteRenderer.sprite = secondSprite;
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(finalCutsceneSceneName);
    }

    private IEnumerator MuteWorldAudioRoutine(float duration)
    {
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        float[] startVolumes = new float[allSources.Length];

        for (int i = 0; i < allSources.Length; i++)
        {
            if (PersistentMusicPlayer.Instance != null && allSources[i].gameObject == PersistentMusicPlayer.Instance.gameObject)
            {
                startVolumes[i] = -1f;
            }
            else
            {
                startVolumes[i] = allSources[i].volume;
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            for (int i = 0; i < allSources.Length; i++)
            {
                if (startVolumes[i] >= 0f && allSources[i] != null)
                {
                    allSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, percent);
                }
            }
            yield return null;
        }

        for (int i = 0; i < allSources.Length; i++)
        {
            if (startVolumes[i] >= 0f && allSources[i] != null)
            {
                allSources[i].Stop();
            }
        }
    }
}