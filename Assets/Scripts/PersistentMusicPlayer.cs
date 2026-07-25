using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PersistentMusicPlayer : MonoBehaviour
{
    public static PersistentMusicPlayer Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    private float maxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (audioSource != null) maxVolume = audioSource.volume;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartMusicFadeIn(float duration)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            StartCoroutine(FadeInRoutine(duration));
        }
    }

    private IEnumerator FadeInRoutine(float duration)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / duration);
            yield return null;
        }
        audioSource.volume = maxVolume;
    }

    public void ReduceVolumeForNextSlide(float targetVolumePercent, float duration)
    {
        StartCoroutine(FadeToVolumeRoutine(targetVolumePercent * maxVolume, duration));
    }

    private IEnumerator FadeToVolumeRoutine(float targetVol, float duration)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }
        audioSource.volume = targetVol;
    }

    public void FadeOutAndLoadScene(string sceneName, float fadeOutDuration)
    {
        StartCoroutine(FadeOutAndSwitchRoutine(sceneName, fadeOutDuration));
    }

    private IEnumerator FadeOutAndSwitchRoutine(string sceneName, float fadeOutDuration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();

        Destroy(gameObject);
        SceneManager.LoadScene(sceneName);
    }
}