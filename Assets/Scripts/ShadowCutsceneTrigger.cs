using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ShadowCutsceneTrigger : MonoBehaviour
{
    [Header("Shadow Setup")]
    [SerializeField] private GameObject shadowObject;

    [Header("Cutscene Camera & Vignette")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float zoomedCamSize = 1.4f;
    [SerializeField] private float targetVignetteIntensity = 0.6f;

    [Header("Cutscene Audio")]
    [SerializeField] private AudioSource cutsceneAudioSource;
    [SerializeField] private float maxCutsceneVolume = 0.8f;

    [Header("Player Freeze Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Sprite customCutsceneSprite;

    [Header("Player Speed Settings")]
    [SerializeField] private float cutsceneMoveSpeed = 2f;
    [SerializeField] private float postCutsceneMoveSpeed = 8f;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 2.0f;

    private bool hasTriggered = false;
    private PlayerController2D playerController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;

            // Grab the player controller and slow them down
            playerController = collision.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.SetMoveSpeed(cutsceneMoveSpeed);
            }

            if (shadowObject != null)
            {
                shadowObject.SetActive(true);
            }

            StartCoroutine(CutsceneRoutine());
        }
    }

    private IEnumerator CutsceneRoutine()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameplayLocked = true;
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if (playerSpriteRenderer != null && customCutsceneSprite != null)
        {
            playerSpriteRenderer.sprite = customCutsceneSprite;
        }

        float startCamSize = mainCamera != null ? mainCamera.orthographicSize : 2f;

        Vignette vignette = null;
        float startVignette = 0f;
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            if (vignette != null)
            {
                startVignette = vignette.intensity.value;
                vignette.intensity.overrideState = true;
            }
        }

        if (cutsceneAudioSource != null)
        {
            cutsceneAudioSource.volume = 0f;
            cutsceneAudioSource.Play();
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fadeInDuration;

            if (mainCamera != null)
            {
                mainCamera.orthographicSize = Mathf.Lerp(startCamSize, zoomedCamSize, percent);
            }

            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(startVignette, targetVignetteIntensity, percent);
            }

            if (cutsceneAudioSource != null)
            {
                cutsceneAudioSource.volume = Mathf.Lerp(0f, maxCutsceneVolume, percent);
            }

            yield return null;
        }

        if (mainCamera != null) mainCamera.orthographicSize = zoomedCamSize;

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fadeOutDuration;

            if (mainCamera != null)
            {
                mainCamera.orthographicSize = Mathf.Lerp(zoomedCamSize, startCamSize, percent);
            }

            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(targetVignetteIntensity, startVignette, percent);
            }

            if (cutsceneAudioSource != null)
            {
                cutsceneAudioSource.volume = Mathf.Lerp(maxCutsceneVolume, 0f, percent);
            }

            yield return null;
        }

        if (mainCamera != null) mainCamera.orthographicSize = startCamSize;
        if (vignette != null) vignette.intensity.value = startVignette;
        if (cutsceneAudioSource != null)
        {
            cutsceneAudioSource.volume = 0f;
            cutsceneAudioSource.Stop();
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameplayLocked = false;
        }

        // Restore the player's normal speed after the cutscene finishes!
        if (playerController != null)
        {
            playerController.SetMoveSpeed(postCutsceneMoveSpeed);
        }
    }
}