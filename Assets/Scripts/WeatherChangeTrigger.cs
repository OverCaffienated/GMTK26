using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class WeatherChangeTrigger : MonoBehaviour
{
    [Header("Environment References")]
    [SerializeField] private ParticleSystem[] rainParticleSystems;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer cloudsBackground;
    [SerializeField] private GameObject shadowObject;

    [Header("Audio Fade References")]
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private GameObject thunderManagerObject;
    [SerializeField] private AudioSource happyMusicAudioSource;
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private GameObject randomAmbientSFXObject;
    [SerializeField] private float targetHappyMusicVolume = 0.5f;
    [SerializeField] private float targetWindVolume = 0.3f;

    [Header("Weather Transition Settings")]
    [SerializeField] private float transitionDuration = 4.0f;
    [SerializeField] private Color newSkyColor = new Color(0.85f, 0.9f, 0.7f);
    [SerializeField] private float newLightIntensity = 1.2f;
    [SerializeField] private float targetCloudAlpha = 1f;

    [Header("Post-Processing Settings")]
    [SerializeField] private float targetVignetteIntensity = 0f;
    [SerializeField] private float targetBloomIntensity = 0f;

    [Header("Player Stat Changes")]
    [SerializeField] private float newMovementSpeed = 3f;
    [SerializeField] private float newJumpHeight = 5f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;

            PlayerController2D playerScript = collision.GetComponent<PlayerController2D>();
            if (playerScript != null)
            {
                playerScript.SetMoveSpeed(newMovementSpeed);
            }

            if (shadowObject != null)
            {
                shadowObject.SetActive(false);
            }

            if (thunderManagerObject != null)
            {
                thunderManagerObject.SetActive(false);
            }

            if (randomAmbientSFXObject != null)
            {
                randomAmbientSFXObject.SetActive(true);
            }

            StartCoroutine(WeatherTransitionRoutine());
        }
    }

    private IEnumerator WeatherTransitionRoutine()
    {
        float timeElapsed = 0f;

        Color startLightColor = globalLight != null ? globalLight.color : Color.white;
        float startIntensity = globalLight != null ? globalLight.intensity : 1f;
        Color startCamColor = mainCamera != null ? mainCamera.backgroundColor : Color.black;

        float startRainVolume = rainAudioSource != null ? rainAudioSource.volume : 1f;

        if (happyMusicAudioSource != null)
        {
            happyMusicAudioSource.volume = 0f;
            happyMusicAudioSource.Play();
        }

        if (windAudioSource != null)
        {
            windAudioSource.volume = 0f;
            windAudioSource.Play();
        }

        Color cloudColor = Color.white;
        float startCloudAlpha = 0f;
        if (cloudsBackground != null)
        {
            cloudColor = cloudsBackground.color;
            startCloudAlpha = cloudColor.a;
        }

        float[] startRainRates = new float[rainParticleSystems.Length];
        ParticleSystem.EmissionModule[] emissions = new ParticleSystem.EmissionModule[rainParticleSystems.Length];

        for (int i = 0; i < rainParticleSystems.Length; i++)
        {
            if (rainParticleSystems[i] != null)
            {
                emissions[i] = rainParticleSystems[i].emission;
                startRainRates[i] = emissions[i].rateOverTime.constant;
            }
        }

        Vignette vignette = null;
        Bloom bloom = null;
        float startVignette = 0f;
        float startBloomIntensity = 1f;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out bloom);

            if (vignette != null)
            {
                startVignette = vignette.intensity.value;
                vignette.intensity.overrideState = true;
            }
            if (bloom != null)
            {
                startBloomIntensity = bloom.intensity.value;
                bloom.intensity.overrideState = true;
            }
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

            if (rainAudioSource != null)
            {
                rainAudioSource.volume = Mathf.Lerp(startRainVolume, 0f, percent);
            }

            if (happyMusicAudioSource != null)
            {
                happyMusicAudioSource.volume = Mathf.Lerp(0f, targetHappyMusicVolume, percent);
            }

            if (windAudioSource != null)
            {
                windAudioSource.volume = Mathf.Lerp(0f, targetWindVolume, percent);
            }

            if (cloudsBackground != null)
            {
                cloudColor.a = Mathf.Lerp(startCloudAlpha, targetCloudAlpha, percent);
                cloudsBackground.color = cloudColor;
            }

            for (int i = 0; i < rainParticleSystems.Length; i++)
            {
                if (rainParticleSystems[i] != null)
                {
                    emissions[i].rateOverTime = Mathf.Lerp(startRainRates[i], 0f, percent);
                }
            }

            if (vignette != null) vignette.intensity.value = Mathf.Lerp(startVignette, targetVignetteIntensity, percent);
            if (bloom != null) bloom.intensity.value = Mathf.Lerp(startBloomIntensity, targetBloomIntensity, percent);

            yield return null;
        }

        if (globalLight != null)
        {
            globalLight.color = newSkyColor;
            globalLight.intensity = newLightIntensity;
        }
        if (mainCamera != null) mainCamera.backgroundColor = newSkyColor;

        if (rainAudioSource != null)
        {
            rainAudioSource.volume = 0f;
            rainAudioSource.Stop();
        }

        if (happyMusicAudioSource != null) happyMusicAudioSource.volume = targetHappyMusicVolume;
        if (windAudioSource != null) windAudioSource.volume = targetWindVolume;

        if (cloudsBackground != null)
        {
            cloudColor.a = targetCloudAlpha;
            cloudsBackground.color = cloudColor;
        }

        for (int i = 0; i < rainParticleSystems.Length; i++)
        {
            if (rainParticleSystems[i] != null)
            {
                emissions[i].rateOverTime = 0f;
                rainParticleSystems[i].Stop();
            }
        }

        if (vignette != null) vignette.intensity.value = targetVignetteIntensity;
        if (bloom != null) bloom.intensity.value = targetBloomIntensity;
    }
}