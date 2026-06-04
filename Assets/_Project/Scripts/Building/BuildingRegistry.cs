using System.Collections.Generic;
using UnityEngine;

public class BuildingRegistry : MonoBehaviour
{
    static BuildingRegistry instance;

    readonly Dictionary<BuildingKind, int> counts = new Dictionary<BuildingKind, int>();

    public static BuildingRegistry Instance => instance;

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

    public static bool HasBuilding(BuildingKind kind)
    {
        if (kind == BuildingKind.None)
        {
            return true;
        }

        return instance != null && instance.GetCount(kind) > 0;
    }

    public static int GetBuildingCount(BuildingKind kind)
    {
        if (kind == BuildingKind.None || instance == null)
        {
            return kind == BuildingKind.None ? 1 : 0;
        }

        return instance.GetCount(kind);
    }

    public void Register(BuildingKind kind)
    {
        if (kind == BuildingKind.None)
        {
            return;
        }

        counts.TryGetValue(kind, out int current);
        counts[kind] = current + 1;
        OnBuildingCountChanged?.Invoke(kind, counts[kind]);
    }

    public void Unregister(BuildingKind kind)
    {
        if (kind == BuildingKind.None || !counts.TryGetValue(kind, out int current))
        {
            return;
        }

        current = Mathf.Max(0, current - 1);
        if (current <= 0)
        {
            counts.Remove(kind);
        }
        else
        {
            counts[kind] = current;
        }

        OnBuildingCountChanged?.Invoke(kind, current);
    }

    int GetCount(BuildingKind kind)
    {
        return counts.TryGetValue(kind, out int count) ? count : 0;
    }

    public event System.Action<BuildingKind, int> OnBuildingCountChanged;
}
