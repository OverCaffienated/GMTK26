using UnityEngine;
using System.Collections;

public class RandomThunder : MonoBehaviour
{
    [SerializeField] private AudioSource thunderAudioSource;
    [SerializeField] private AudioClip[] thunderClips;
    [SerializeField] private float minTimeBetweenThunders = 8f;
    [SerializeField] private float maxTimeBetweenThunders = 20f;

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

            if (thunderAudioSource != null && thunderClips.Length > 0)
            {
                AudioClip randomClip = thunderClips[Random.Range(0, thunderClips.Length)];
                thunderAudioSource.PlayOneShot(randomClip);
            }
        }
    }
}