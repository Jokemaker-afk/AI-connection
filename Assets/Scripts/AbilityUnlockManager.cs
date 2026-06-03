using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityUnlockManager : MonoBehaviour
{
    readonly HashSet<GameplayAbility> unlocked = new HashSet<GameplayAbility>();

    public void Unlock(GameplayAbility ability)
    {
        if (ability == GameplayAbility.None)
        {
            return;
        }

        foreach (GameplayAbility flag in EnumerateFlags(ability))
        {
            unlocked.Add(flag);
        }
    }

    public bool HasAbility(GameplayAbility ability)
    {
        if (ability == GameplayAbility.None)
        {
            return true;
        }

        foreach (GameplayAbility flag in EnumerateFlags(ability))
        {
            if (!unlocked.Contains(flag))
            {
                return false;
            }
        }

        return true;
    }

    public void EnsureInheritedAbilitiesForScene(string sceneName)
    {
        Unlock(GameplaySceneCatalog.GetInheritedAbilities(sceneName));
    }

    public void ImportAbilities(GameplayAbility abilityFlags)
    {
        unlocked.Clear();
        Unlock(abilityFlags);
    }

    public GameplayAbility ExportAbilities()
    {
        GameplayAbility flags = GameplayAbility.None;
        foreach (GameplayAbility ability in unlocked)
        {
            flags |= ability;
        }

        return flags;
    }

    static IEnumerable<GameplayAbility> EnumerateFlags(GameplayAbility ability)
    {
        foreach (GameplayAbility flag in System.Enum.GetValues(typeof(GameplayAbility)))
        {
            if (flag == GameplayAbility.None)
            {
                continue;
            }

            if ((ability & flag) == flag)
            {
                yield return flag;
            }
        }
    }
}
