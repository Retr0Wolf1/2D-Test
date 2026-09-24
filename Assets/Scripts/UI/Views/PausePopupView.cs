// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

public class PausePopupView : ViewBase
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;

    protected override void OnShow()
    {
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