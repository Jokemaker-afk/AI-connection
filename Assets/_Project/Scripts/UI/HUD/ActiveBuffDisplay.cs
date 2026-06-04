using System;
using UnityEngine;

public struct ActiveBuffDisplay
{
    public BuffType Type;
    public float RemainingSeconds;
    public float TotalSeconds;
    public bool UseCountdown;
    public string OverrideLabel;
}
