using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ThoughtZoneTrigger : MonoBehaviour
{
    [Header("Camera & Zoom")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float targetZoomSize = 3.5f;
    [SerializeField] private float zoomTransitionDuration = 1.0f;
    [SerializeField] private float zoomHoldDuration = 2.0f;
    private float originalCamSize;

    [Header("Shadow Control")]
    [SerializeField] private GameObject shadowObject;

    [Header("Player Slow Effect")]
    [SerializeField] private float slowedMoveSpeed = 2.0f;
    [SerializeField] private float slowDuration = 2.0f;
    [SerializeField] private float normalMoveSpeed = 5.0f;

    [Header("Player Sprite & Animator")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Sprite thoughtSprite;

    [Header("Vignette Settings")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float targetVignetteIntensity = 0.4f;
    [SerializeField] private float vignetteTransitionDuration = 1.0f;
    private float originalVignetteIntensity = 0f;
    private Vignette vignetteComponent;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource thoughtAudioSource;

    private bool hasTriggered = false;

    private void Start()
    {
        if (mainCamera != null)
        {
            originalCamSize = mainCamera.orthographicSize;
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            if (globalVolume.profile.TryGet(out vignetteComponent))
            {
                originalVignetteIntensity = vignetteComponent.intensity.value;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ThoughtSequenceRoutine(collision));
        }
    }

    private IEnumerator ThoughtSequenceRoutine(Collider2D playerCollision)
    {
        PlayerController2D playerScript = playerCollision.GetComponent<PlayerController2D>();
        if (playerScript != null)
        {
            playerScript.SetMoveSpeed(slowedMoveSpeed);
        }

        if (shadowObject != null)
        {
            shadowObject.SetActive(false);
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if (playerSpriteRenderer != null && thoughtSprite != null)
        {
            playerSpriteRenderer.sprite = thoughtSprite;
        }

        if (thoughtAudioSource != null)
        {
            thoughtAudioSource.Play();
        }

        if (mainCamera != null)
        {
            StartCoroutine(ZoomCameraRoutine(originalCamSize, targetZoomSize));
        }

        if (vignetteComponent != null)
        {
            StartCoroutine(VignetteRoutine(originalVignetteIntensity, targetVignetteIntensity));
        }

        yield return new WaitForSeconds(slowDuration);

        if (playerScript != null)
        {
            playerScript.SetMoveSpeed(normalMoveSpeed);
        }

        yield return new WaitForSeconds(zoomHoldDuration);

        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        if (shadowObject != null)
        {
            shadowObject.SetActive(true);
        }

        if (mainCamera != null)
        {
            StartCoroutine(ZoomCameraRoutine(mainCamera.orthographicSize, originalCamSize));
        }

        if (vignetteComponent != null)
        {
            StartCoroutine(VignetteRoutine(vignetteComponent.intensity.value, originalVignetteIntensity));
        }
    }

    private IEnumerator ZoomCameraRoutine(float startSize, float endSize)
    {
        float elapsed = 0f;
        while (elapsed < zoomTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / zoomTransitionDuration;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, percent);
            yield return null;
        }
        mainCamera.orthographicSize = endSize;
    }

    private IEnumerator VignetteRoutine(float startVal, float endVal)
    {
        float elapsed = 0f;
        vignetteComponent.intensity.overrideState = true;

        while (elapsed < vignetteTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / vignetteTransitionDuration;
            vignetteComponent.intensity.value = Mathf.Lerp(startVal, endVal, percent);
            yield return null;
        }
        vignetteComponent.intensity.value = endVal;
    }
}