using System;
using UnityEngine;

/// <summary>
/// Lightweight event bus for interaction/progression hooks (Signal Relay, portals, gates, etc.).
/// </summary>
public static class GameEventManager
{
    public static event Action<string> OnEventTriggered;

    public static void TriggerEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }

        Debug.Log($"[GameEvent] Triggered: {eventId}");
        OnEventTriggered?.Invoke(eventId);
    }
}
