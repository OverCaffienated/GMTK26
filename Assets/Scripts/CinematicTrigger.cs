using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.SceneManagement;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Ending Scene Transition")]
    [SerializeField] private bool isEndGameTrigger = false;
    [SerializeField] private string endingSceneName = "EndScene";

    [Header("Player Speed Transition")]
    [SerializeField] private bool changePlayerSpeed = false;
    [SerializeField] private float newPlayerSpeed = 2f;

    [Header("Atmosphere Transition")]
    [SerializeField] private bool changeAtmosphere = false;
    [SerializeField] private float transitionDuration = 3f;

    [Space(10)]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private float targetRainEmissionRate = 0f;

    [Space(10)]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float targetLightIntensity = 1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        if (isEndGameTrigger)
        {
            SceneManager.LoadScene(endingSceneName);
            return;
        }

        if (changePlayerSpeed)
        {
            PlayerController2D pc = other.GetComponent<PlayerController2D>();
            if (pc != null) pc.SetMoveSpeed(newPlayerSpeed);
        }

        if (changeAtmosphere)
        {
            StartCoroutine(TransitionAtmosphereRoutine());
        }
    }

    private IEnumerator TransitionAtmosphereRoutine()
    {
        float timeElapsed = 0f;
        float startRainRate = 0f;
        ParticleSystem.EmissionModule emission = default;

        if (rainParticles != null)
        {
            emission = rainParticles.emission;
            startRainRate = emission.rateOverTime.constant;
        }

        float startLightIntensity = 0f;
        if (globalLight != null)
        {
            startLightIntensity = globalLight.intensity;
        }

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / transitionDuration;

            if (rainParticles != null)
            {
                emission.rateOverTime = Mathf.Lerp(startRainRate, targetRainEmissionRate, t);
            }

            if (globalLight != null)
            {
                globalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, t);
            }

            yield return null;
        }
    }
}