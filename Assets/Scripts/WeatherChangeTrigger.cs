using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class WeatherChangeTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private Camera mainCamera;

    [Header("Weather Transition Settings")]
    [SerializeField] private float transitionDuration = 4.0f;
    [SerializeField] private Color newSkyColor = new Color(0.85f, 0.9f, 0.7f);
    [SerializeField] private float newLightIntensity = 1.2f;

    [Header("Post-Processing Settings")]
    [SerializeField] private float targetVignetteIntensity = 0f;
    [SerializeField] private float targetBloomThreshold = 0.5f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(WeatherTransitionRoutine());
        }
    }

    private IEnumerator WeatherTransitionRoutine()
    {
        float timeElapsed = 0f;

        Color startLightColor = globalLight != null ? globalLight.color : Color.white;
        float startIntensity = globalLight != null ? globalLight.intensity : 1f;

        Color startCamColor = mainCamera != null ? mainCamera.backgroundColor : Color.black;

        var emission = rainParticles.emission;
        float startRainRate = emission.rateOverTime.constant;

        Vignette vignette = null;
        Bloom bloom = null;
        float startVignette = 0f;
        float startBloomThreshold = 1f;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out bloom);

            if (vignette != null) startVignette = vignette.intensity.value;
            if (bloom != null) startBloomThreshold = bloom.threshold.value;
        }

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float percent = timeElapsed / transitionDuration;

            if (globalLight != null)
            {
                globalLight.color = Color.Lerp(startLightColor, newSkyColor, percent);
                globalLight.intensity = Mathf.Lerp(startIntensity, newLightIntensity, percent);
            }
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.Lerp(startCamColor, newSkyColor, percent);
            }

            if (rainParticles != null)
            {
                emission.rateOverTime = Mathf.Lerp(startRainRate, 0f, percent);
            }

            if (vignette != null) vignette.intensity.value = Mathf.Lerp(startVignette, targetVignetteIntensity, percent);
            if (bloom != null) bloom.threshold.value = Mathf.Lerp(startBloomThreshold, targetBloomThreshold, percent);

            yield return null;
        }

        if (globalLight != null)
        {
            globalLight.color = newSkyColor;
            globalLight.intensity = newLightIntensity;
        }
        if (mainCamera != null) mainCamera.backgroundColor = newSkyColor;

        if (rainParticles != null)
        {
            emission.rateOverTime = 0f;
            rainParticles.Stop();
        }
        if (vignette != null) vignette.intensity.value = targetVignetteIntensity;
        if (bloom != null) bloom.threshold.value = targetBloomThreshold;
    }
}