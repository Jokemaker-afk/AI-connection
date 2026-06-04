using System.IO;
using UnityEngine;

/// <summary>
/// Optional disk persistence for character progression. Runtime snapshot remains primary during play.
/// </summary>
public static class CharacterSaveFileUtility
{
    const string SaveFileName = "character_progression.json";

    public static string GetSaveFilePath(string characterId)
    {
        string safeId = string.IsNullOrEmpty(characterId) ? "default" : characterId;
        return Path.Combine(Application.persistentDataPath, $"character_{safeId}_{SaveFileName}");
    }

    public static bool TrySaveToDisk(PlayerProgressionSaveData data)
    {
        try
        {
            string path = GetSaveFilePath(data.CharacterId);
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(path, json);
            GameplayCore.Instance?.Log($"[CharacterSave] Written: {path}");
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"[CharacterSave] Save failed: {exception.Message}");
            return false;
        }
    }

    public static bool TryLoadFromDisk(string characterId, out PlayerProgressionSaveData data)
    {
        data = default;
        string path = GetSaveFilePath(characterId);
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<PlayerProgressionSaveData>(json);
            PlayerProgressionState.MigrateSaveData(ref data);
            GameplayCore.Instance?.Log($"[CharacterSave] Loaded: {path}");
            return data.HasPersistentState;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"[CharacterSave] Load failed: {exception.Message}");
            return false;
        }
    }
}
