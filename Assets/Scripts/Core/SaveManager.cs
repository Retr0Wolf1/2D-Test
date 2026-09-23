// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;

public static class SaveManager
{
    private const string SeedKey = "Save_Seed";
    private const string FoodKey = "Save_Food";
    private const string LevelKey = "Save_Level";
    private const string PlayerXKey = "Save_PlayerX";
    private const string PlayerYKey = "Save_PlayerY";
    private const string HasSaveKey = "Save_HasSave";
    private const string BestLevelKey = "Save_BestLevel";

    public static bool HasSave => PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    public static int BestLevel => PlayerPrefs.GetInt(BestLevelKey, 0);

    public static void SaveGame(int seed, int food, int level, Vector2Int playerPos)
    {
        PlayerPrefs.SetInt(SeedKey, seed);
        PlayerPrefs.SetInt(FoodKey, food);
        PlayerPrefs.SetInt(LevelKey, level);
        PlayerPrefs.SetInt(PlayerXKey, playerPos.x);
        PlayerPrefs.SetInt(PlayerYKey, playerPos.y);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
    }

    public static int LoadSeed()
    {
        return PlayerPrefs.GetInt(SeedKey, 0);
    }

    public static int LoadFood()
    {
        return PlayerPrefs.GetInt(FoodKey, 20);
    }

    public static int LoadLevel()
    {
        return PlayerPrefs.GetInt(LevelKey, 1);
    }

    public static Vector2Int LoadPlayerPos()
    {
        return new Vector2Int(
            PlayerPrefs.GetInt(PlayerXKey, 1),
            PlayerPrefs.GetInt(PlayerYKey, 1)
        );
    }

    public static void TrySaveBestLevel(int level)
    {
        if (level > BestLevel)
        {
            PlayerPrefs.SetInt(BestLevelKey, level);
            PlayerPrefs.Save();
        }
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SeedKey);
        PlayerPrefs.DeleteKey(FoodKey);
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.SetInt(HasSaveKey, 0);
        PlayerPrefs.Save();
    }
}