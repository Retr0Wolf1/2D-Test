// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class GameOverPopupView : ViewBase
{
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private TextMeshProUGUI _gameOverTitle;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    protected override void OnShow()
    {
        _gameOverTitle.text = GetText("gameover_title");
        _bestScoreText.text = GetText("best_label") + ": " + SaveManager.BestLevel + " " + GetText("days_label");

        SetButtonText(_restartButton, "restart_button");
        SetButtonText(_mainMenuButton, "mainmenu_button");

        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    protected override void OnHide()
    {
        _restartButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
    }

    private void SetButtonText(Button button, string key)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = GetText(key);
        }
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