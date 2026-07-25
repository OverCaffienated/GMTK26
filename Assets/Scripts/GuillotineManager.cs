using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GuillotineManager : MonoBehaviour
{
    public static GuillotineManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject guillotineVisualRoot;
    [SerializeField] private GameObject glintObject;
    [SerializeField] private Animator guillotineAnimator;
    [SerializeField] private AudioSource guillotineAudioSource;
    [SerializeField] private string permanentDeathSceneName = "PermanentDeathScene";

    [Header("Timing Config")]
    [SerializeField] private float totalDuration = 4.0f;
    [SerializeField] private float glintWindowDuration = 0.6f;

    private bool canParryNow = false;
    private bool sequenceEnded = false;
    private PlayerCombatOrParry deadPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (guillotineVisualRoot != null) guillotineVisualRoot.SetActive(false);
    }

    private void Update()
    {
        if (!sequenceEnded && canParryNow)
        {
            bool parryPressed = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                                (Keyboard.current != null && (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame));

            if (parryPressed)
            {
                SuccessParry();
            }
        }
    }

    public void StartGuillotineEvent(PlayerCombatOrParry player)
    {
        deadPlayer = player;
        sequenceEnded = false;
        canParryNow = false;

        Time.timeScale = 0f;

        if (guillotineVisualRoot != null) guillotineVisualRoot.SetActive(true);
        if (glintObject != null) glintObject.SetActive(false);

        if (guillotineAudioSource != null) guillotineAudioSource.Play();

        StartCoroutine(GuillotineSequence());
    }

    private IEnumerator GuillotineSequence()
    {
        yield return null;
        if (guillotineAnimator != null) guillotineAnimator.SetTrigger("Fall");

        float randomDelay = Random.Range(totalDuration - 1.8f, totalDuration - 0.8f);

        yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, randomDelay));

        canParryNow = true;
        if (glintObject != null) glintObject.SetActive(true);

        yield return new WaitForSecondsRealtime(glintWindowDuration);

        canParryNow = false;
        if (glintObject != null) glintObject.SetActive(false);

        float remainingTime = totalDuration - randomDelay - glintWindowDuration;
        if (remainingTime > 0f) yield return new WaitForSecondsRealtime(remainingTime);

        if (!sequenceEnded)
        {
            sequenceEnded = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(permanentDeathSceneName);
        }
    }

    private void SuccessParry()
    {
        sequenceEnded = true;
        if (glintObject != null) glintObject.SetActive(false);
        if (guillotineVisualRoot != null) guillotineVisualRoot.SetActive(false);

        Time.timeScale = 1f;

        if (deadPlayer != null)
        {
            deadPlayer.ReviveWithLowHP();
        }

        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null)
        {
            shadow.ApplyGuillotineRespawn();
        }
    }
}