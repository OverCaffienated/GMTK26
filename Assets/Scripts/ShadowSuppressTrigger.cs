using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ShadowSuppressTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Volume globalVolume;

    [Header("Settings")]
    [SerializeField] private float zoomedCamSize = 1.4f;
    [SerializeField] private float targetVignetteIntensity = 0.4f;
    [SerializeField] private float suppressionDuration = 4.0f;
    [SerializeField] private float transitionSpeed = 2.0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(SuppressRoutine());
        }
    }

    private IEnumerator SuppressRoutine()
    {
        if (shadowObject != null)
        {
            shadowObject.SetActive(false);
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

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            if (mainCamera != null) mainCamera.orthographicSize = Mathf.Lerp(startCamSize, zoomedCamSize, elapsed);
            if (vignette != null) vignette.intensity.value = Mathf.Lerp(startVignette, targetVignetteIntensity, elapsed);
            yield return null;
        }

        yield return new WaitForSeconds(suppressionDuration);

        if (shadowObject != null)
        {
            shadowObject.SetActive(true);
        }

        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            if (mainCamera != null) mainCamera.orthographicSize = Mathf.Lerp(zoomedCamSize, startCamSize, elapsed);
            if (vignette != null) vignette.intensity.value = Mathf.Lerp(targetVignetteIntensity, startVignette, elapsed);
            yield return null;
        }

        if (mainCamera != null) mainCamera.orthographicSize = startCamSize;
        if (vignette != null) vignette.intensity.value = startVignette;
    }
}