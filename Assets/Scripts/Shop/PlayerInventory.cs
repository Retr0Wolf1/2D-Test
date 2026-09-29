// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using UnityEngine;

// Купленное хранится отдельно от сохранения забега: оно удаляется при смерти.
public static class PlayerInventory
{
    private const string ReviveTokensKey = "inventory_revive_tokens";
    private const string NoAdsKey = "inventory_no_ads";

    public static event Action Changed;

    public static int ReviveTokens => PlayerPrefs.GetInt(ReviveTokensKey, 0);
    public static bool HasNoAds => PlayerPrefs.GetInt(NoAdsKey, 0) == 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Domain reload в редакторе отключён: подписчики живут между запусками Play.
        Changed = null;
    }

    public static void AddReviveTokens(int amount)
    {
        PlayerPrefs.SetInt(ReviveTokensKey, ReviveTokens + amount);
        PlayerPrefs.Save();

        Changed?.Invoke();
    }

    public static bool TrySpendReviveToken()
    {
        if (ReviveTokens <= 0)
        {
            return false;
        }

        PlayerPrefs.SetInt(ReviveTokensKey, ReviveTokens - 1);
        PlayerPrefs.Save();

        Changed?.Invoke();

        return true;
    }

    public static void SetNoAds(bool value)
    {
        if (HasNoAds == value)
        {
            return;
        }

        PlayerPrefs.SetInt(NoAdsKey, value ? 1 : 0);
        PlayerPrefs.Save();

        Changed?.Invoke();
    }
}
