// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public static class LocalizationManager
{
    public enum Language { English, Russian }

    private const string LanguageKey = "SelectedLanguage";

    private static readonly Dictionary<string, string[]> Translations = new Dictionary<string, string[]>()
    {
        { "start",       new[] { "Start Game",     "Начать игру" } },
        { "continue",    new[] { "Continue",       "Продолжить" } },
        { "settings",    new[] { "Settings",       "Настройки" } },
        { "quit",        new[] { "Quit",           "Выход" } },
        { "best",        new[] { "Best",           "Рекорд" } },
        { "days",        new[] { "days",           "дней" } },
        { "food",        new[] { "Food",           "Еда" } },
        { "music",       new[] { "Music",          "Музыка" } },
        { "sounds",      new[] { "Sounds",         "Звуки" } },
        { "back",        new[] { "Back",           "Назад" } },
        { "resume",      new[] { "Resume",         "Продолжить" } },
        { "mainmenu",    new[] { "Main Menu",      "Главное меню" } },
        { "restart",     new[] { "Restart",        "Заново" } },
        { "gameover",    new[] { "Game Over!",     "Игра окончена!" } },
        { "goexit",      new[] { "All enemies defeated! Go to the exit!", "Все враги убиты! Иди к выходу!" } },
        { "paused",      new[] { "Paused",         "Пауза" } },
        { "language",    new[] { "Language",       "Язык" } },

        { "hint", new[]
            {
                "Goal: Find the exit!\n\n" +
                "Move: Arrow Keys\n" +
                "Attack: Move into enemy/wall\n" +
                "Collect food to survive\n" +
                "Kill all enemies to open the exit",

                "Цель: Найди выход!\n\n" +
                "Движение: Стрелки\n" +
                "Атака: Иди на врага/стену\n" +
                "Собирай еду чтобы выжить\n" +
                "Убей всех врагов чтобы открыть выход"
            }
        },
    };

    public static event System.Action OnLanguageChanged;

    public static Language Current
    {
        get => (Language)PlayerPrefs.GetInt(LanguageKey, 0);
        set
        {
            PlayerPrefs.SetInt(LanguageKey, (int)value);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    public static string Get(string key)
    {
        if (!Translations.ContainsKey(key))
        {
            return key;
        }

        string[] variants = Translations[key];
        int index = (int)Current;

        if (index < 0 || index >= variants.Length)
        {
            return variants[0];
        }

        return variants[index];
    }
}