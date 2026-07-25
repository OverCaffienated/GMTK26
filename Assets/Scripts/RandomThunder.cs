using UnityEngine;
using System.Collections;

public class RandomThunder : MonoBehaviour
{
    [System.Serializable]
    public struct ThunderSound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] private AudioSource thunderAudioSource;
    [SerializeField] private ThunderSound[] thunderSounds;
    [SerializeField] private float minTimeBetweenThunders = 8f;
    [SerializeField] private float maxTimeBetweenThunders = 20f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.1f;

    private void Start()
    {
        StartCoroutine(ThunderRoutine());
    }

    private IEnumerator ThunderRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenThunders, maxTimeBetweenThunders);
            yield return new WaitForSeconds(waitTime);

            if (thunderAudioSource != null && thunderSounds.Length > 0)
            {
                yield return new WaitUntil(() => !thunderAudioSource.isPlaying);

                ThunderSound randomThunder = thunderSounds[Random.Range(0, thunderSounds.Length)];

                if (randomThunder.clip != null)
                {
                    thunderAudioSource.pitch = Random.Range(minPitch, maxPitch);
                    thunderAudioSource.PlayOneShot(randomThunder.clip, randomThunder.volume);
                }
            }
        }
    }
}