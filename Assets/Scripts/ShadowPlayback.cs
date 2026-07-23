using UnityEngine;
using System.Collections.Generic;

public class ShadowPlayback : MonoBehaviour
{
    public ShadowRecorder recorder;
    public float delaySeconds = 4f;
    public float catchupSpeedMultiplier = 1.5f;
    public float normalPlaybackMultiplier = 1f;
    public float farDistance = 7f;
    public float nearDistance = 4f;

    private float playbackMultiplier = 1f;
    private float localTime;

    void Update()
    {
        if (recorder.frames.Count < 2) return;

        float targetTime = recorder.frames[recorder.frames.Count - 1].time - delaySeconds;

        float gap = Vector2.Distance(transform.position, recorder.target.position);
        playbackMultiplier = gap > farDistance ? catchupSpeedMultiplier :
                             gap < nearDistance ? normalPlaybackMultiplier :
                             playbackMultiplier;

        localTime = Mathf.MoveTowards(localTime, targetTime, Time.deltaTime * playbackMultiplier);

        ApplyFrameAtTime(localTime);
    }

    void ApplyFrameAtTime(float t)
    {
        List<ShadowFrame> frames = recorder.frames;
        for (int i = 0; i < frames.Count - 1; i++)
        {
            if (frames[i].time <= t && frames[i + 1].time >= t)
            {
                ShadowFrame a = frames[i];
                ShadowFrame b = frames[i + 1];
                float lerp = Mathf.InverseLerp(a.time, b.time, t);

                transform.position = Vector2.Lerp(a.position, b.position, lerp);

                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (a.facingRight ? 1 : -1);
                transform.localScale = scale;
                return;
            }
        }
    }
}