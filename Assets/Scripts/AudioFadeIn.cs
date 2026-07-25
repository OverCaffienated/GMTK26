using UnityEngine;
using System.Collections;

public class AudioFadeIn : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 3.0f;
    [SerializeField] private float targetVolume = 0.5f;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            StartCoroutine(FadeInRoutine());
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float percent = timeElapsed / fadeDuration;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, percent);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}