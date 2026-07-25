using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct SlideData
{
    [TextArea(3, 5)] public string text;
    public Sprite image;
}

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI slideText;
    [SerializeField] private Image slideImageDisplay;
    [SerializeField] private CanvasGroup contentCanvasGroup;

    [Header("Cutscene Settings")]
    [SerializeField] private SlideData[] slides;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private string nextSceneName;

    [Header("End Cutscene Music Controls")]
    [SerializeField] private bool isEndCutscene = false;
    [SerializeField] private float musicFadeOutDuration = 2.0f;

    private int currentSlideIndex = 0;
    private bool isWaitingForClick = false;

    private void Start()
    {
        contentCanvasGroup.alpha = 0f;
        StartCoroutine(PlaySlide());
    }

    private void Update()
    {
        if (isWaitingForClick)
        {
            bool skipPressed = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                skipPressed = true;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                skipPressed = true;

            if (skipPressed)
            {
                isWaitingForClick = false;
            }
        }
    }

    private IEnumerator PlaySlide()
    {
        while (currentSlideIndex < slides.Length)
        {
            slideText.text = slides[currentSlideIndex].text;

            if (slides[currentSlideIndex].image != null)
            {
                slideImageDisplay.sprite = slides[currentSlideIndex].image;
                slideImageDisplay.color = Color.white;
            }
            else
            {
                slideImageDisplay.color = Color.clear;
            }

            while (contentCanvasGroup.alpha < 1f)
            {
                contentCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            contentCanvasGroup.alpha = 1f;

            if (isEndCutscene && PersistentMusicPlayer.Instance != null && slides.Length > 1)
            {
                float volumePercent = 1f - ((float)currentSlideIndex / (slides.Length - 1));
                volumePercent = Mathf.Clamp(volumePercent, 0.1f, 1f);
                PersistentMusicPlayer.Instance.ReduceVolumeForNextSlide(volumePercent, 1.5f);
            }

            isWaitingForClick = true;
            while (isWaitingForClick)
            {
                yield return null;
            }

            while (contentCanvasGroup.alpha > 0f)
            {
                contentCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            contentCanvasGroup.alpha = 0f;

            currentSlideIndex++;
            yield return new WaitForSeconds(0.5f);
        }
        if (isEndCutscene && PersistentMusicPlayer.Instance != null)
        {
            PersistentMusicPlayer.Instance.FadeOutAndLoadScene(nextSceneName, musicFadeOutDuration);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}