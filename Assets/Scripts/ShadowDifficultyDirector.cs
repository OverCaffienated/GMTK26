using System.Collections.Generic;
using UnityEngine;

public class ShadowDifficultyDirector : MonoBehaviour
{
    public ShadowPlayback shadow;
    public float eventWindow = 300f;
    public int threshold = 3;
    public float delayReduction = 0.4f;
    public float minDelay = 2.5f;
    public float farDistance = 5f;

    private Queue<float> catchupEvents = new Queue<float>();
    private bool wasCatchingUp;

    void Update()
    {
        if (shadow == null) return;

        PlayerController2D player = Object.FindAnyObjectByType<PlayerController2D>();
        if (player == null) return;

        bool catchingUp = shadow.enabled &&
                          Vector2.Distance(shadow.transform.position, player.transform.position) > farDistance;

        if (catchingUp && !wasCatchingUp)
            catchupEvents.Enqueue(Time.time);

        wasCatchingUp = catchingUp;

        while (catchupEvents.Count > 0 && Time.time - catchupEvents.Peek() > eventWindow)
            catchupEvents.Dequeue();

        if (catchupEvents.Count >= threshold)
        {
            shadow.delaySeconds = Mathf.Max(minDelay, shadow.delaySeconds - delayReduction);
            catchupEvents.Clear();
        }
    }
}