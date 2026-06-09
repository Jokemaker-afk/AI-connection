using System;
using System.Collections.Generic;
using UnityEngine;

public class Level7TaskProgressTracker : MonoBehaviour, ISceneTaskTracker
{
    static Level7TaskProgressTracker instance;

    readonly HashSet<string> completedTasks = new HashSet<string>();

    public static Level7TaskProgressTracker Instance => instance;

    public event Action OnProgressChanged;
    public event Action<string> OnTaskCompleted;

    public const string HitMeleeDummyTaskId = "hit_melee_dummy";
    public const string HitRangedTargetTaskId = "hit_ranged_target";
    public const string DefeatBasicWalkerTaskId = "defeat_basic_walker";

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
        return IsTaskComplete(HitMeleeDummyTaskId)
            && IsTaskComplete(HitRangedTargetTaskId)
            && IsTaskComplete(DefeatBasicWalkerTaskId);
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

    public void ResetForSceneEntry()
    {
        completedTasks.Clear();
        OnProgressChanged?.Invoke();
    }
}
