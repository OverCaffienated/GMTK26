using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class RandomLightFlicker : MonoBehaviour
{
    [SerializeField] private Light2D targetLight;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float minFlickerSpeed = 0.05f;
    [SerializeField] private float maxFlickerSpeed = 0.2f;

    private void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light2D>();
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
        }
    }
}