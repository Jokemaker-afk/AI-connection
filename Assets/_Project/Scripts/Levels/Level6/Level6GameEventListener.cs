using UnityEngine;

public class Level6GameEventListener : MonoBehaviour
{
    void OnEnable()
    {
        GameEventManager.OnEventTriggered += HandleGameEvent;
    }

    void OnDisable()
    {
        GameEventManager.OnEventTriggered -= HandleGameEvent;
    }

    void HandleGameEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }

        switch (eventId)
        {
            case "signal_relay_repaired":
                GameplayCore.Instance?.Log("Level6 event: signal relay repaired — portal can activate after all tasks.");
                break;
        }
    }
}
