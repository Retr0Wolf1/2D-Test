// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class PausePopupView : ViewBase
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private TextMeshProUGUI _pauseTitle;

    protected override void OnShow()
    {
        _pauseTitle.text = GetText("paused_title");

        SetButtonText(_resumeButton, "resume_button");
        SetButtonText(_settingsButton, "settings_button");
        SetButtonText(_mainMenuButton, "mainmenu_button");

        _resumeButton.onClick.AddListener(OnResumeClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    protected override void OnHide()
    {
        _resumeButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
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

    private void OnResumeClicked()
    {
        ViewManager.Instance.ClosePopup<PausePopupView>();
    }

    private void OnSettingsClicked()
    {
        ViewManager.Instance.OpenPopup<SettingsPopupView>();
    }

    private void OnMainMenuClicked()
    {
        ViewManager.Instance.CloseAll();
        GameManager.Instance.ReturnToMainMenu();
        ViewManager.Instance.OpenPage<MainPageView>();
    }
}