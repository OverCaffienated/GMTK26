using UnityEngine;
using System.Collections;

public class AudioTriggerBox : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clipToPlay;
    [Range(0f, 1f)][SerializeField] private float maxVolume = 1f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasPlayed && collision.CompareTag("Player"))
        {
            hasPlayed = true;

            if (audioSource != null && clipToPlay != null)
            {
                StartCoroutine(PlayWithFadesRoutine());
            }
        }
    }

    private IEnumerator PlayWithFadesRoutine()
    {
        audioSource.clip = clipToPlay;
        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = maxVolume;

        float remainingTime = clipToPlay.length - fadeInDuration - fadeOutDuration;
        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(maxVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}