// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using UnityEngine;

// Game ID и ID размещений берутся из дашборда Unity Ads:
// cloud.unity.com → Monetization → Приложения / Размещения.
[CreateAssetMenu(fileName = "AdsConfig", menuName = "Game/Ads Config")]
public class AdsConfig : ScriptableObject
{
    public const string ResourcePath = "AdsConfig";

    [Serializable]
    public class PlatformKeys
    {
        public string GameId;
        public string RewardedPlacementId;
        public string InterstitialPlacementId;
        public string BannerPlacementId;
    }

    [SerializeField] private PlatformKeys _android = new PlatformKeys
    {
        RewardedPlacementId = "BP_Rewarded_Android",
        InterstitialPlacementId = "BP_Interstitial_Android",
        BannerPlacementId = "BP_Banner_Android",
    };

    [SerializeField] private PlatformKeys _ios = new PlatformKeys
    {
        RewardedPlacementId = "Rewarded_iOS",
        InterstitialPlacementId = "Interstitial_iOS",
        BannerPlacementId = "Banner_iOS",
    };

    [Tooltip("Тестовая реклама. Перед релизом выключить.")]
    [SerializeField] private bool _testMode = true;

    [Header("Interstitial")]
    [Tooltip("Показывать межстраничную рекламу раз в N смертей.")]
    [Min(1)]
    [SerializeField] private int _interstitialEveryDeaths = 2;

    [Tooltip("Минимальная пауза между полноэкранными показами (включая ревардед), сек.")]
    [Min(0)]
    [SerializeField] private float _fullscreenCooldown = 90f;

    public bool TestMode => _testMode;
    public int InterstitialEveryDeaths => _interstitialEveryDeaths;
    public float FullscreenCooldown => _fullscreenCooldown;

    public PlatformKeys Current
    {
        get
        {
#if UNITY_IOS
            return _ios;
#else
            return _android;
#endif
        }
    }
}
