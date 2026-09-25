// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFolderName = "Save";
    private const string SaveFileName = "save.json";
    private const string WriteTestFileName = ".write_test";

    private static string _cachedPath;

    private static string SavePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_cachedPath))
            {
                return _cachedPath;
            }

            // Путь рядом с игрой
            var nearGame = Path.Combine(Application.dataPath, "..", SaveFolderName);

            try
            {
                // Пробуем записать тестовый файл рядом с игрой
                var testPath = Path.Combine(Application.dataPath, "..", WriteTestFileName);
                File.WriteAllText(testPath, "test");
                File.Delete(testPath);

                // Успех — используем папку рядом с игрой
                _cachedPath = Path.Combine(nearGame, SaveFileName);

                Debug.Log("Save path (near game): " + _cachedPath);
            }
            catch (Exception)
            {
                // Нельзя — используем persistentDataPath
                _cachedPath = Path.Combine(Application.persistentDataPath, SaveFolderName, SaveFileName);

                Debug.Log("Save path (persistent): " + _cachedPath);
            }

            EnsureDirectory();

            return _cachedPath;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void SaveGame(SaveData data)
    {
        EnsureDirectory();

        var json = JsonUtility.ToJson(data, true);

        File.WriteAllText(SavePath, json);

        Debug.Log("Saved to: " + SavePath);
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        var json = File.ReadAllText(SavePath);

        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void TrySaveBestLevel(int level)
    {
        var data = LoadGame();

        if (data == null)
        {
            data = new SaveData();
        }

        if (level > data.BestLevel)
        {
            data.BestLevel = level;
            SaveGame(data);
        }
    }

    public static int GetBestLevel()
    {
        var data = LoadGame();

        return data != null ? data.BestLevel : 0;
    }

    public static void ClearSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save deleted.");
        }
    }

    private static void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(SavePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}