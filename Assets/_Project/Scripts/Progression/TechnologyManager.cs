using System.Collections.Generic;
using UnityEngine;

public class TechnologyManager : MonoBehaviour
{
    static TechnologyManager instance;

    readonly HashSet<TechnologyKind> unlocked = new HashSet<TechnologyKind>();

    public static TechnologyManager Instance => instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Unlock(TechnologyKind.BasicSurvival);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool IsUnlocked(TechnologyKind technology)
    {
        if (technology == TechnologyKind.None)
        {
            return true;
        }

        return unlocked.Contains(technology);
    }

    public bool HasAll(params TechnologyKind[] technologies)
    {
        if (technologies == null || technologies.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < technologies.Length; i++)
        {
            if (!IsUnlocked(technologies[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void Unlock(TechnologyKind technology)
    {
        if (technology == TechnologyKind.None)
        {
            return;
        }

        if (unlocked.Add(technology))
        {
            OnTechnologyUnlocked?.Invoke(technology);
        }
    }

    public event System.Action<TechnologyKind> OnTechnologyUnlocked;

    public TechnologyKind[] ExportUnlocked()
    {
        if (unlocked.Count == 0)
        {
            return System.Array.Empty<TechnologyKind>();
        }

        var results = new TechnologyKind[unlocked.Count];
        unlocked.CopyTo(results);
        return results;
    }

    public void ImportUnlocked(TechnologyKind[] technologies)
    {
        unlocked.Clear();
        Unlock(TechnologyKind.BasicSurvival);
        if (technologies == null)
        {
            return;
        }

        for (int i = 0; i < technologies.Length; i++)
        {
            Unlock(technologies[i]);
        }
    }
}
