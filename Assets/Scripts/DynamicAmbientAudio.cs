using UnityEngine;
using System.Collections;

public class DynamicAmbientAudio : MonoBehaviour
{
    [Header("Ambient Trees & Wind Settings")]
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private float fadeInDuration = 3.0f;
    [SerializeField] private float minVolume = 0.05f;
    [SerializeField] private float maxVolume = 0.4f;
    [SerializeField] private float minSwellDuration = 4.0f;
    [SerializeField] private float maxSwellDuration = 9.0f;

    private void Start()
    {
        if (ambientAudioSource != null)
        {

            ambientAudioSource.volume = 0f;
            if (!ambientAudioSource.isPlaying) ambientAudioSource.Play();

            StartCoroutine(FadeInAndSwellRoutine());
        }
    }

    private IEnumerator FadeInAndSwellRoutine()
    {

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            ambientAudioSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / fadeInDuration);
            yield return null;
        }
        ambientAudioSource.volume = maxVolume;

        while (true)
        {
            float targetVolume = Random.Range(minVolume, maxVolume);
            float duration = Random.Range(minSwellDuration, maxSwellDuration);
            float startVolume = ambientAudioSource.volume;
            elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }
            ambientAudioSource.volume = targetVolume;

            yield return new WaitForSeconds(Random.Range(3f, 8f));
        }
    }
}