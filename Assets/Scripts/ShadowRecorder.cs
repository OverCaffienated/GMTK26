using System.Collections.Generic;
using UnityEngine;

public class ShadowRecorder : MonoBehaviour
{
    public Transform target;
    public Rigidbody2D targetRb;
    public float sampleInterval = 0.02f;
    public float maxRecordSeconds = 360f;

    public List<ShadowFrame> frames = new List<ShadowFrame>();

    private float timer;
    private float elapsed;

    void Update()
    {
        elapsed += Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= sampleInterval)
        {
            timer = 0f;

            frames.Add(new ShadowFrame
            {
                time = elapsed,
                position = target.position,
                velocity = targetRb.linearVelocity,
                facingRight = target.localScale.x >= 0
            });
        }

        float cutoff = elapsed - maxRecordSeconds;
        while (frames.Count > 0 && frames[0].time < cutoff)
            frames.RemoveAt(0);
    }
}