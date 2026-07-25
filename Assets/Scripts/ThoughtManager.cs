using UnityEngine;
using TMPro;
using System.Collections;

public class ThoughtManager : MonoBehaviour
{
    public static ThoughtManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI thoughtText;
    [SerializeField] private GameObject thoughtPanel;
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float displayDurationAfterFinish = 2.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private CanvasGroup panelCanvasGroup;
    private Coroutine currentTypeRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (thoughtPanel != null)
        {
            panelCanvasGroup = thoughtPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = thoughtPanel.AddComponent<CanvasGroup>();
            }
            thoughtPanel.SetActive(false);
        }
    }

    public void ShowThought(string text)
    {
        if (currentTypeRoutine != null) StopCoroutine(currentTypeRoutine);
        currentTypeRoutine = StartCoroutine(TypewriterRoutine(text));
    }

    private IEnumerator TypewriterRoutine(string text)
    {
        if (thoughtPanel != null) thoughtPanel.SetActive(true);
        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;
        if (thoughtText != null) thoughtText.text = "";

        if (typingAudioSource != null)
        {
            typingAudioSource.loop = true;
            typingAudioSource.Play();
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (thoughtText != null) thoughtText.text += text[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        if (typingAudioSource != null)
        {
            typingAudioSource.Stop();
        }

        yield return new WaitForSeconds(displayDurationAfterFinish);

        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            panelCanvasGroup.alpha = 0f;
        }

        if (thoughtPanel != null) thoughtPanel.SetActive(false);
    }
}