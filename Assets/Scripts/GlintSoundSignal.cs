using UnityEngine;

public class GlintSoundSignal : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip glintSound;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    private void OnEnable()
    {
        if (audioSource != null && glintSound != null)
        {
            audioSource.PlayOneShot(glintSound, volume);
        }
    }
}