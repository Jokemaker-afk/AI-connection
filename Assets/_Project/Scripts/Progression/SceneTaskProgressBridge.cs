using UnityEngine;

public interface ISceneTaskTracker
{
    void RegisterTaskComplete(string taskId);
}

public static class SceneTaskProgressBridge
{
    public static void RegisterTaskComplete(string taskId)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            return;
        }

        var trackers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < trackers.Length; i++)
        {
            if (trackers[i] is ISceneTaskTracker tracker)
            {
                tracker.RegisterTaskComplete(taskId);
            }
        }
    }
}
