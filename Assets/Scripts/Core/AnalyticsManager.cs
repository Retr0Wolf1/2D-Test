// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;

public static class AnalyticsManager
{
    private static bool _isInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async void Initialize()
    {
        try
        {
            await UnityServices.InitializeAsync();

            // TODO: заменить на экран согласия, если игра пойдёт в регионы с GDPR/COPPA.
            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = ConsentStatus.Granted,
            });

            _isInitialized = true;
            Debug.Log("[Analytics] Initialized");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Analytics] Initialization failed: {e.Message}");
        }
    }

    public static void RunStarted(bool isContinue, int level)
    {
        Record(new CustomEvent("runStarted")
        {
            { "startType", isContinue ? "continue" : "new" },
            { "level", level },
        });
    }

    public static void LevelCompleted(int level, int foodLeft)
    {
        Record(new CustomEvent("levelCompleted")
        {
            { "level", level },
            { "foodLeft", foodLeft },
        });
    }

    public static void EnemyKilled(int level)
    {
        Record(new CustomEvent("enemyKilled")
        {
            { "level", level },
        });
    }

    public static void PlayerRevived(int level, bool isFromAd)
    {
        Record(new CustomEvent("playerRevived")
        {
            { "level", level },
            { "source", isFromAd ? "ad" : "token" },
        });
    }

    public static void ItemPurchased(string productId)
    {
        Record(new CustomEvent("itemPurchased")
        {
            { "productId", productId },
        });
    }

    public static void RunEnded(int level, int bestLevel)
    {
        Record(new CustomEvent("runEnded")
        {
            { "level", level },
            { "bestLevel", bestLevel },
        });

        // Смерть часто заканчивается закрытием игры, поэтому отправляем сразу.
        Flush();
    }

    private static void Record(CustomEvent analyticsEvent)
    {
        if (!_isInitialized)
        {
            return;
        }

        AnalyticsService.Instance.RecordEvent(analyticsEvent);
    }

    private static void Flush()
    {
        if (!_isInitialized)
        {
            return;
        }

        AnalyticsService.Instance.Flush();
    }
}
