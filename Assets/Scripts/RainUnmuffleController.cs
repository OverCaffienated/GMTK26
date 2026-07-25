using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class RainUnmuffleController : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private float startCutoffFrequency = 600f;
    [SerializeField] private float targetCutoffFrequency = 22000f;
    [SerializeField] private float unmuffleDuration = 3.0f;

    private void Awake()
    {
        if (lowPassFilter == null) lowPassFilter = GetComponent<AudioLowPassFilter>();
    }

    private void Start()
    {
        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = startCutoffFrequency;
            StartCoroutine(UnmuffleRoutine());
        }
    }

    private IEnumerator UnmuffleRoutine()
    {
        float elapsed = 0f;

        while (elapsed < unmuffleDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / unmuffleDuration;
            lowPassFilter.cutoffFrequency = Mathf.Lerp(startCutoffFrequency, targetCutoffFrequency, percent);
            yield return null;
        }

        lowPassFilter.cutoffFrequency = targetCutoffFrequency;
    }
}