using UnityEngine;

public class GlintAnimator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float growDuration = 0.15f;
    [SerializeField] private Vector3 maxScale = Vector3.one;

    private float timer = 0f;

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        timer = 0f;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);

        if (timer < growDuration)
        {
            timer += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(timer / growDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, percent);
        }
        else
        {
            transform.localScale = maxScale;
        }
    }
}