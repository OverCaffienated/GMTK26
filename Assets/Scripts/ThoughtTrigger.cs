using UnityEngine;

public class ThoughtTrigger : MonoBehaviour
{
    [TextArea(3, 6)]
    [SerializeField] private string thoughtMessage;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered && triggerOnlyOnce) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            if (ThoughtManager.Instance != null)
            {
                ThoughtManager.Instance.ShowThought(thoughtMessage);
            }
        }
    }
}