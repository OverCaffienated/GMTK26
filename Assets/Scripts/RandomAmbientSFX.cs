using UnityEngine;
using System.Collections;

public class RandomAmbientSFX : MonoBehaviour
{
    [System.Serializable]
    public struct AmbientSound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AmbientSound[] ambientSounds;
    [SerializeField] private float minInterval = 4f;
    [SerializeField] private float maxInterval = 12f;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    private void OnEnable()
    {
        StartCoroutine(PlayRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator PlayRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (audioSource != null && ambientSounds.Length > 0)
            {
                yield return new WaitUntil(() => !audioSource.isPlaying);

                AmbientSound randomSound = ambientSounds[Random.Range(0, ambientSounds.Length)];

                if (randomSound.clip != null)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    audioSource.PlayOneShot(randomSound.clip, randomSound.volume);

                    yield return new WaitForSeconds(randomSound.clip.length);
                }
            }
        }
    }
}