// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;

// Три формата:
// - Rewarded — игрок сам выбирает смотреть рекламу за награду (воскрешение).
// - Interstitial — полноэкранная реклама в естественной паузе (выход с экрана смерти).
// - Banner — полоска вверху экрана, только в главном меню, чтобы не мешать джойстику.
public static class AdsManager
{
    private const float MaxRetryDelay = 64f;

    private static readonly Listener _listener = new Listener();

    private static AdsConfig _config;
    private static AdsConfig.PlatformKeys _keys;

    private static bool _isRewardedLoaded;
    private static bool _isInterstitialLoaded;
    private static bool _isBannerLoaded;

    private static float _rewardedRetryDelay = 1f;
    private static float _interstitialRetryDelay = 1f;

    private static Action _pendingRewardCallback;
    private static Action _pendingInterstitialCallback;
    private static bool _isBannerWanted;
    private static bool _isShowingFullscreen;
    private static float _lastFullscreenTime = float.NegativeInfinity;
    private static int _deathsSinceInterstitial;

    public static event Action RewardedAvailabilityChanged;

    public static bool IsRewardedReady => Advertisement.isInitialized && !_isShowingFullscreen && _isRewardedLoaded;

    // После загрузки сцены: заглушка рекламы в редакторе создаёт свой EventSystem
    // со старым Input Manager, если в сцене его ещё нет.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        _isRewardedLoaded = false;
        _isInterstitialLoaded = false;
        _isBannerLoaded = false;
        _isShowingFullscreen = false;
        _isBannerWanted = false;
        _deathsSinceInterstitial = 0;
        _lastFullscreenTime = float.NegativeInfinity;
        _pendingRewardCallback = null;
        _pendingInterstitialCallback = null;
        _rewardedRetryDelay = 1f;
        _interstitialRetryDelay = 1f;

        // Domain reload в редакторе отключён: статика и подписчики живут между запусками Play.
        RewardedAvailabilityChanged = null;

        PlayerInventory.Changed += OnInventoryChanged;

        if (!Advertisement.isSupported)
        {
            return;
        }

        _config = Resources.Load<AdsConfig>(AdsConfig.ResourcePath);

        if (_config == null)
        {
            Debug.LogWarning("[Ads] AdsConfig not found in Resources, ads disabled");
            return;
        }

        _keys = _config.Current;

        if (string.IsNullOrEmpty(_keys.GameId))
        {
            Debug.LogWarning("[Ads] Game ID is empty, ads disabled");
            return;
        }

        // SDK уже инициализирован прошлым запуском Play — колбэк повторно не придёт.
        if (Advertisement.isInitialized)
        {
            OnInitialized();
            return;
        }

        Advertisement.Initialize(_keys.GameId, _config.TestMode, _listener);
    }

    public static void ShowRewarded(Action onRewarded)
    {
        if (!IsRewardedReady)
        {
            return;
        }

        _pendingRewardCallback = onRewarded;
        _isShowingFullscreen = true;
        _isRewardedLoaded = false;
        Advertisement.Show(_keys.RewardedPlacementId, _listener);

        RewardedAvailabilityChanged?.Invoke();
    }

    // Вызывается при уходе с экрана смерти. Реклама показывается не чаще,
    // чем раз в N смертей и не раньше кулдауна после прошлого полноэкранного показа.
    public static void ShowInterstitialAfterDeath(Action onFinished)
    {
        _deathsSinceInterstitial++;

        bool isDue = _config != null && _deathsSinceInterstitial >= _config.InterstitialEveryDeaths;
        bool isCooldownOver = _config != null && Time.realtimeSinceStartup - _lastFullscreenTime >= _config.FullscreenCooldown;
        bool isReady = Advertisement.isInitialized && !_isShowingFullscreen && _isInterstitialLoaded;

        if (PlayerInventory.HasNoAds || !isDue || !isCooldownOver || !isReady)
        {
            onFinished?.Invoke();
            return;
        }

        _deathsSinceInterstitial = 0;
        _pendingInterstitialCallback = onFinished;
        _isShowingFullscreen = true;
        _isInterstitialLoaded = false;
        Advertisement.Show(_keys.InterstitialPlacementId, _listener);
    }

    public static void ShowBanner()
    {
        _isBannerWanted = true;

        if (_isBannerLoaded && !PlayerInventory.HasNoAds)
        {
            Advertisement.Banner.Show(_keys.BannerPlacementId);
        }
    }

    public static void HideBanner()
    {
        _isBannerWanted = false;

        if (_isBannerLoaded)
        {
            Advertisement.Banner.Hide();
        }
    }

    // No Ads купили, пока баннер на экране.
    private static void OnInventoryChanged()
    {
        if (PlayerInventory.HasNoAds && _isBannerLoaded)
        {
            Advertisement.Banner.Hide();
        }
    }

    private static void OnInitialized()
    {
        Debug.Log("[Ads] Initialized");

        LoadIfSet(_keys.RewardedPlacementId);
        LoadIfSet(_keys.InterstitialPlacementId);
        LoadBanner();
    }

    private static void LoadIfSet(string placementId)
    {
        if (!string.IsNullOrEmpty(placementId))
        {
            Advertisement.Load(placementId, _listener);
        }
    }

    private static void LoadBanner()
    {
        if (string.IsNullOrEmpty(_keys.BannerPlacementId))
        {
            return;
        }

        // Сверху: внизу меню баннер закрывал рекорд.
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
        Advertisement.Banner.Load(_keys.BannerPlacementId, new BannerLoadOptions
        {
            loadCallback = () =>
            {
                _isBannerLoaded = true;

                if (_isBannerWanted && !PlayerInventory.HasNoAds)
                {
                    Advertisement.Banner.Show(_keys.BannerPlacementId);
                }
            },
            errorCallback = message => Debug.LogWarning($"[Ads] Banner load failed: {message}"),
        });
    }

    private static void OnLoaded(string placementId)
    {
        if (placementId == _keys.RewardedPlacementId)
        {
            _isRewardedLoaded = true;
            _rewardedRetryDelay = 1f;
            RewardedAvailabilityChanged?.Invoke();
        }
        else if (placementId == _keys.InterstitialPlacementId)
        {
            _isInterstitialLoaded = true;
            _interstitialRetryDelay = 1f;
        }
    }

    private static void OnLoadFailed(string placementId, string message)
    {
        Debug.LogWarning($"[Ads] {placementId} load failed: {message}");

        if (placementId == _keys.RewardedPlacementId)
        {
            RetryLoad(placementId, _rewardedRetryDelay);
            _rewardedRetryDelay = Mathf.Min(_rewardedRetryDelay * 2f, MaxRetryDelay);
        }
        else if (placementId == _keys.InterstitialPlacementId)
        {
            RetryLoad(placementId, _interstitialRetryDelay);
            _interstitialRetryDelay = Mathf.Min(_interstitialRetryDelay * 2f, MaxRetryDelay);
        }
    }

    private static void OnShowFinished(string placementId, bool isCompleted)
    {
        _isShowingFullscreen = false;
        _lastFullscreenTime = Time.realtimeSinceStartup;

        Advertisement.Load(placementId, _listener);

        if (placementId == _keys.RewardedPlacementId)
        {
            var callback = _pendingRewardCallback;
            _pendingRewardCallback = null;

            // Награда только за досмотренную рекламу.
            if (isCompleted)
            {
                callback?.Invoke();
            }
        }
        else if (placementId == _keys.InterstitialPlacementId)
        {
            var callback = _pendingInterstitialCallback;
            _pendingInterstitialCallback = null;
            callback?.Invoke();
        }

        RewardedAvailabilityChanged?.Invoke();
    }

    private static async void RetryLoad(string placementId, float delay)
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));

        if (!Application.isPlaying)
        {
            return;
        }

        Advertisement.Load(placementId, _listener);
    }

    private sealed class Listener : IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public void OnInitializationComplete()
        {
            OnInitialized();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogWarning($"[Ads] Initialization failed: {error} {message}");
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            OnLoaded(placementId);
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            OnLoadFailed(placementId, $"{error} {message}");
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"[Ads] {placementId} show failed: {error} {message}");
            OnShowFinished(placementId, false);
        }

        public void OnUnityAdsShowStart(string placementId) { }

        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
        {
            OnShowFinished(placementId, state == UnityAdsShowCompletionState.COMPLETED);
        }
    }
}
