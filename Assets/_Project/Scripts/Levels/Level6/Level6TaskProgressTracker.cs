using System;
using System.Collections.Generic;
using UnityEngine;

public class Level6TaskProgressTracker : MonoBehaviour, ISceneTaskTracker
{
    static Level6TaskProgressTracker instance;

    readonly HashSet<string> completedTasks = new HashSet<string>();

    public static Level6TaskProgressTracker Instance => instance;

    public event Action OnProgressChanged;
    public event Action<string> OnTaskCompleted;

    public const string MineOnceTaskId = "mine_once";
    public const string ChopOnceTaskId = "chop_once";
    public const string RepairSignalRelayTaskId = "repair_signal_relay";

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool IsTaskComplete(string taskId)
    {
        return !string.IsNullOrEmpty(taskId) && completedTasks.Contains(taskId);
    }

    public bool AreAllTutorialTasksComplete()
    {
        return IsTaskComplete(MineOnceTaskId)
            && IsTaskComplete(ChopOnceTaskId)
            && IsTaskComplete(RepairSignalRelayTaskId);
    }

    public void RegisterTaskComplete(string taskId)
    {
        if (string.IsNullOrEmpty(taskId) || !completedTasks.Add(taskId))
        {
            return;
        }

        OnTaskCompleted?.Invoke(taskId);
        OnProgressChanged?.Invoke();
    }
}
