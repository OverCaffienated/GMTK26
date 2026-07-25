using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GuillotineManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject glintObject;
    [SerializeField] private Animator guillotineAnimator;
    [SerializeField] private AudioSource guillotineAudioSource;
    [SerializeField] private string gameplaySceneName = "MainGame";
    [SerializeField] private string permanentDeathSceneName = "PermanentDeathScene";

    [Header("Timing Config")]
    [SerializeField] private float totalDuration = 4.0f;
    [SerializeField] private float glintWindowDuration = 0.6f;

    private bool canParryNow = false;
    private bool sequenceEnded = false;

    private void Start()
    {
        if (glintObject != null) glintObject.SetActive(false);

        if (guillotineAnimator != null)
        {
            guillotineAnimator.SetTrigger("Fall");
        }

        if (guillotineAudioSource != null)
        {
            guillotineAudioSource.Play();
        }

        StartCoroutine(GuillotineSequence());
    }

    private void Update()
    {
        bool parryPressed = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                            (Keyboard.current != null && (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame));

        if (canParryNow && !sequenceEnded && parryPressed)
        {
            SuccessParry();
        }
    }

    private IEnumerator GuillotineSequence()
    {
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

        Time.timeScale = 1f;

        PlayerCombatOrParry playerCombat = FindAnyObjectByType<PlayerCombatOrParry>();
        if (playerCombat != null)
        {

        }

        ShadowPlayback shadow = FindAnyObjectByType<ShadowPlayback>();
        if (shadow != null)
        {
            shadow.ApplyGuillotineRespawn();
        }

        SceneManager.UnloadSceneAsync("GuillotineDeathScene");
    }
}