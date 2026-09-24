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

    protected override void OnShow()
    {
        _bestScoreText.text = GetText("best_label") + ": " + SaveManager.BestLevel + " " + GetText("days_label");

        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    protected override void OnHide()
    {
        _restartButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
    }

    private string GetText(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", key);
    }

    private void OnRestartClicked()
    {
        ViewManager.Instance.ClosePopup<GameOverPopupView>();
        GameManager.Instance.StartNewGame();
    }

    private void OnMainMenuClicked()
    {
        ViewManager.Instance.CloseAll();
        GameManager.Instance.ReturnToMainMenu();
        ViewManager.Instance.OpenPage<MainPageView>();
    }
}