// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using TMPro;

public class GameOverPopupView : ViewBase
{
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _reviveButton;
    [SerializeField] private TextMeshProUGUI _reviveText;
    [SerializeField] private Button _tokenReviveButton;
    [SerializeField] private TextMeshProUGUI _tokenReviveText;

    private bool _isPurchasing;

    protected override void OnShow()
    {
        _bestScoreText.text = GetText("best_label") + ": " + SaveManager.GetBestLevel() + " " + GetText("days_label");

        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        _reviveButton.onClick.AddListener(OnReviveClicked);
        _tokenReviveButton.onClick.AddListener(OnTokenReviveClicked);

        _isPurchasing = false;
        SetExitButtonsInteractable(true);

        AdsManager.RewardedAvailabilityChanged += UpdateReviveButton;
        PurchaseManager.ReadyChanged += UpdateTokenReviveButton;
        PlayerInventory.Changed += UpdateTokenReviveButton;

        UpdateReviveButton();
        UpdateTokenReviveButton();
    }

    protected override void OnHide()
    {
        _restartButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
        _reviveButton.onClick.RemoveAllListeners();
        _tokenReviveButton.onClick.RemoveAllListeners();

        AdsManager.RewardedAvailabilityChanged -= UpdateReviveButton;
        PurchaseManager.ReadyChanged -= UpdateTokenReviveButton;
        PlayerInventory.Changed -= UpdateTokenReviveButton;
    }

    private string GetText(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", key);
    }

    private void UpdateReviveButton()
    {
        int revivesLeft = GameManager.Instance.RevivesLeft;

        _reviveButton.gameObject.SetActive(revivesLeft > 0);
        _reviveButton.interactable = AdsManager.IsRewardedReady;
        _reviveText.text = GetText("revive_label") + " (" + revivesLeft + ")";
    }

    // Есть жетоны — тратим жетон, нет — предлагаем купить пачку.
    private void UpdateTokenReviveButton()
    {
        int tokens = PlayerInventory.ReviveTokens;

        if (tokens > 0)
        {
            _tokenReviveButton.gameObject.SetActive(true);
            _tokenReviveButton.interactable = !_isPurchasing;
            _tokenReviveText.text = GetText("revive_token_label") + " (" + tokens + ")";
            return;
        }

        string price = PurchaseManager.GetPrice(PurchaseManager.RevivePackId);

        _tokenReviveButton.gameObject.SetActive(!string.IsNullOrEmpty(price));
        _tokenReviveButton.interactable = !_isPurchasing && PurchaseManager.CanBuy(PurchaseManager.RevivePackId);
        _tokenReviveText.text = GetText("revive_buy_label") + " " + price;
    }

    private void SetExitButtonsInteractable(bool interactable)
    {
        _restartButton.interactable = interactable;
        _mainMenuButton.interactable = interactable;
    }

    private void OnReviveClicked()
    {
        _reviveButton.interactable = false;

        AdsManager.ShowRewarded(() =>
        {
            // Награда может прийти уже после закрытия окна, тогда не воскрешаем.
            if (IsOpen)
            {
                GameManager.Instance.Revive(true);
            }
        });
    }

    private void OnTokenReviveClicked()
    {
        if (PlayerInventory.TrySpendReviveToken())
        {
            GameManager.Instance.Revive(false);
            return;
        }

        _isPurchasing = true;
        UpdateTokenReviveButton();

        PurchaseManager.Buy(PurchaseManager.RevivePackId, isSuccess =>
        {
            _isPurchasing = false;

            // Если окно уже закрыто, жетоны просто останутся на следующий раз.
            if (isSuccess && IsOpen && PlayerInventory.TrySpendReviveToken())
            {
                GameManager.Instance.Revive(false);
                return;
            }

            if (IsOpen)
            {
                UpdateTokenReviveButton();
            }
        });
    }

    private void OnRestartClicked()
    {
        SetExitButtonsInteractable(false);

        AdsManager.ShowInterstitialAfterDeath(() =>
        {
            ViewManager.Instance.ClosePopup<GameOverPopupView>();
            GameManager.Instance.StartNewGame();
        });
    }

    private void OnMainMenuClicked()
    {
        SetExitButtonsInteractable(false);

        AdsManager.ShowInterstitialAfterDeath(() =>
        {
            ViewManager.Instance.CloseAll();
            GameManager.Instance.ReturnToMainMenu();
            ViewManager.Instance.OpenPage<MainPageView>();
        });
    }
}
