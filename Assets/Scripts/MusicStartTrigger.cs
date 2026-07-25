using UnityEngine;
using System.Collections;

public class MusicStartTrigger : MonoBehaviour
{
    [SerializeField] private PersistentMusicPlayer musicPlayer;
    [SerializeField] private float fadeInDuration = 2.0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            if (musicPlayer != null)
            {
                musicPlayer.StartMusicFadeIn(fadeInDuration);
            }
        }
    }
}