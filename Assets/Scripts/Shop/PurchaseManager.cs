// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

// Товары:
// - no_ads (NonConsumable) — покупается один раз, убирает межстраничную рекламу и баннер.
//   Ревардед остаётся: его игрок включает сам ради награды.
// - revive_pack (Consumable) — можно покупать повторно, даёт жетоны воскрешения.
// ID должны совпадать с ID товаров в Google Play Console / App Store Connect.
public static class PurchaseManager
{
    public const string NoAdsId = "no_ads";
    public const string RevivePackId = "revive_pack";
    public const int RevivesPerPack = 3;

    private static readonly List<ProductDefinition> _products = new List<ProductDefinition>
    {
        new ProductDefinition(NoAdsId, ProductType.NonConsumable),
        new ProductDefinition(RevivePackId, ProductType.Consumable),
    };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Тестовый магазин (Fake Store): в редакторе и в Development Build покупки бесплатные,
    // без Google Play. В релизной сборке этого кода нет — только настоящий магазин.
    // Fake Store отдаёт "$0.01" и заглушку вместо названия, поэтому показываем
    // то, что будет заведено в Play Console.
    private const string TestStoreName = "fake"; // FakeAppStore.Name, класс в пакете internal

    private static readonly Dictionary<string, (string Title, string Price)> _testProducts =
        new Dictionary<string, (string, string)>
        {
            { NoAdsId, ("Убрать рекламу", "199,00 ₽") },
            { RevivePackId, ("3 воскрешения", "99,00 ₽") },
        };
#endif

    private static StoreController _store;
    private static bool _isReady;
    private static Action<bool> _pendingCallback;
    private static string _pendingProductId;

    public static event Action ReadyChanged;

    public static bool IsReady => _isReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static async void Initialize()
    {
        _isReady = false;
        _pendingCallback = null;
        _pendingProductId = null;

        // Domain reload в редакторе отключён: подписчики живут между запусками Play.
        ReadyChanged = null;

        if (_store != null)
        {
            Unsubscribe(_store);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _store = UnityIAPServices.StoreController(TestStoreName);
#else
        _store = UnityIAPServices.StoreController();
#endif

        _store.OnProductsFetched += OnProductsFetched;
        _store.OnProductsFetchFailed += OnProductsFetchFailed;
        _store.OnPurchasesFetched += OnPurchasesFetched;
        _store.OnPurchasePending += OnPurchasePending;
        _store.OnPurchaseFailed += OnPurchaseFailed;
        _store.OnPurchaseDeferred += OnPurchaseDeferred;
        _store.OnStoreDisconnected += OnStoreDisconnected;
        _store.OnStoreConnected += OnStoreConnected;
        _store.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        _store.OnPurchaseConfirmed += OnPurchaseConfirmed;

        try
        {
            await _store.Connect();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[IAP] Connect failed: {e.Message}");
            return;
        }

        _store.FetchProducts(_products);
    }

    // Для кнопок игры: в пиксельном шрифте нет символа ₽.
    public static string GetPrice(string productId)
    {
        return GetStorePrice(productId).Replace("₽", "руб.");
    }

    // Цена в том виде, в каком её отдаёт магазин.
    private static string GetStorePrice(string productId)
    {
        var product = _isReady ? _store.GetProductById(productId) : null;

        if (product == null)
        {
            return string.Empty;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testProducts.TryGetValue(productId, out var testProduct))
        {
            return testProduct.Price;
        }
#endif

        return product.metadata.localizedPriceString;
    }

    public static bool CanBuy(string productId)
    {
        if (!_isReady || _pendingProductId != null)
        {
            return false;
        }

        var product = _store.GetProductById(productId);

        return product != null && product.availableToPurchase;
    }

    public static void Buy(string productId, Action<bool> onFinished)
    {
        if (!CanBuy(productId))
        {
            onFinished?.Invoke(false);
            return;
        }

        _pendingProductId = productId;
        _pendingCallback = onFinished;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Fake Store работает без своего окна (IAP_FAKE_STORE_DEFAULT) и сразу одобряет покупку,
        // поэтому подтверждение показываем в своей шторке под Google Play.
        var title = _testProducts.TryGetValue(productId, out var testProduct) ? testProduct.Title : productId;

        TestBillingSheet.Show(title, Application.productName, GetStorePrice(productId),
            onApprove: () => _store.PurchaseProduct(productId),
            onFail: () => FinishPending(false));
#else
        _store.PurchaseProduct(productId);
#endif
    }

    private static void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"[IAP] Products fetched: {products.Count}");

        // Подтянет уже купленное (No Ads после переустановки) и незавершённые покупки.
        _store.FetchPurchases();

        _isReady = true;
        ReadyChanged?.Invoke();
    }

    private static void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogWarning($"[IAP] Products fetch failed: {failure.FailureReason}");
    }

    private static void OnPurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            if (Contains(order, NoAdsId))
            {
                PlayerInventory.SetNoAds(true);
            }
        }
    }

    private static void OnPurchasePending(PendingOrder order)
    {
        foreach (var item in order.CartOrdered.Items())
        {
            Grant(item.Product.definition.id);
        }

        // Подтверждаем после выдачи: если игра упадёт раньше, магазин пришлёт покупку снова.
        _store.ConfirmPurchase(order);

        FinishPending(true);
    }

    private static void OnPurchaseFailed(FailedOrder order)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {order.FailureReason} {order.Details}");
        FinishPending(false);
    }

    private static void OnPurchaseDeferred(DeferredOrder order)
    {
        // Оплата отложена (например, наличными в терминале). Товар придёт позже через OnPurchasePending.
        Debug.Log("[IAP] Purchase deferred");
        FinishPending(false);
    }

    private static void OnStoreConnected()
    {
        Debug.Log("[IAP] Store connected");
    }

    private static void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogWarning($"[IAP] Purchases fetch failed: {failure.FailureReason} {failure.Message}");
    }

    private static void OnPurchaseConfirmed(Order order)
    {
        Debug.Log("[IAP] Purchase confirmed");
    }

    private static void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.LogWarning($"[IAP] Store disconnected: {description.Message}");

        _isReady = false;
        ReadyChanged?.Invoke();
    }

    private static void Grant(string productId)
    {
        switch (productId)
        {
            case NoAdsId:
                PlayerInventory.SetNoAds(true);
                break;

            case RevivePackId:
                PlayerInventory.AddReviveTokens(RevivesPerPack);
                break;
        }

        AnalyticsManager.ItemPurchased(productId);
    }

    private static void FinishPending(bool isSuccess)
    {
        var callback = _pendingCallback;

        _pendingCallback = null;
        _pendingProductId = null;

        callback?.Invoke(isSuccess);
        ReadyChanged?.Invoke();
    }

    private static bool Contains(Order order, string productId)
    {
        foreach (var item in order.CartOrdered.Items())
        {
            if (item.Product.definition.id == productId)
            {
                return true;
            }
        }

        return false;
    }

    private static void Unsubscribe(StoreController store)
    {
        store.OnProductsFetched -= OnProductsFetched;
        store.OnProductsFetchFailed -= OnProductsFetchFailed;
        store.OnPurchasesFetched -= OnPurchasesFetched;
        store.OnPurchasePending -= OnPurchasePending;
        store.OnPurchaseFailed -= OnPurchaseFailed;
        store.OnPurchaseDeferred -= OnPurchaseDeferred;
        store.OnStoreDisconnected -= OnStoreDisconnected;
        store.OnStoreConnected -= OnStoreConnected;
        store.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        store.OnPurchaseConfirmed -= OnPurchaseConfirmed;
    }
}
