// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPopupView : ViewBase
{
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _settingsTitle;

    protected override void OnShow()
    {
        _settingsTitle.text = LocalizationManager.Get("settings");

        _backButton.onClick.AddListener(OnBackClicked);

        _musicToggle.isOn = AudioManager.Instance.IsMusicOn();
        _sfxToggle.isOn = AudioManager.Instance.IsSFXOn();
        _musicToggle.onValueChanged.AddListener(OnMusicToggled);
        _sfxToggle.onValueChanged.AddListener(OnSFXToggled);

        _languageDropdown.ClearOptions();
        _languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "Русский" });
        _languageDropdown.value = (int)LocalizationManager.Current;
        _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    protected override void OnHide()
    {
        _backButton.onClick.RemoveAllListeners();
        _musicToggle.onValueChanged.RemoveAllListeners();
        _sfxToggle.onValueChanged.RemoveAllListeners();
        _languageDropdown.onValueChanged.RemoveAllListeners();
    }

    private void OnBackClicked() => ViewManager.Instance.ClosePopup<SettingsPopupView>();
    private void OnMusicToggled(bool value) => AudioManager.Instance.SetMusicOn(value);
    private void OnSFXToggled(bool value) => AudioManager.Instance.SetSFXOn(value);
    private void OnLanguageChanged(int index) => LocalizationManager.Current = (LocalizationManager.Language)index;
}