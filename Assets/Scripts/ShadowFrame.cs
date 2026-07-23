using System;
using UnityEngine;

[Serializable]
public struct ShadowFrame
{
    public float time;
    public Vector2 position;
    public Vector2 velocity;
    public bool facingRight;
}