// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using TMPro;

public class MainPageView : ViewBase
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private Button _noAdsButton;
    [SerializeField] private TextMeshProUGUI _noAdsText;

    protected override void OnShow()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _continueButton.onClick.AddListener(OnContinueClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
        _noAdsButton.onClick.AddListener(OnNoAdsClicked);

        _continueButton.gameObject.SetActive(SaveManager.HasSave());

        _bestScoreText.text = GetText("best_label") + ": " + SaveManager.GetBestLevel() + " " + GetText("days_label");

        PurchaseManager.ReadyChanged += UpdateNoAdsButton;
        PlayerInventory.Changed += UpdateNoAdsButton;
        UpdateNoAdsButton();

        AdsManager.ShowBanner();
    }

    protected override void OnHide()
    {
        AdsManager.HideBanner();

        PurchaseManager.ReadyChanged -= UpdateNoAdsButton;
        PlayerInventory.Changed -= UpdateNoAdsButton;

        _startButton.onClick.RemoveAllListeners();
        _continueButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
        _noAdsButton.onClick.RemoveAllListeners();
    }

    private string GetText(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", key);
    }

    private void UpdateNoAdsButton()
    {
        string price = PurchaseManager.GetPrice(PurchaseManager.NoAdsId);

        _noAdsButton.gameObject.SetActive(!PlayerInventory.HasNoAds && !string.IsNullOrEmpty(price));
        _noAdsButton.interactable = PurchaseManager.CanBuy(PurchaseManager.NoAdsId);
        _noAdsText.text = GetText("no_ads_label") + " " + price;
    }

    private void OnNoAdsClicked()
    {
        _noAdsButton.interactable = false;

        // Кнопка скроется сама через PlayerInventory.Changed.
        PurchaseManager.Buy(PurchaseManager.NoAdsId, _ => UpdateNoAdsButton());
    }

    private void OnStartClicked()
    {
        GameManager.Instance.StartNewGame();
    }

    private void OnContinueClicked()
    {
        GameManager.Instance.ContinueGame();
    }

    private void OnSettingsClicked()
    {
        ViewManager.Instance.OpenPopup<SettingsPopupView>();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}