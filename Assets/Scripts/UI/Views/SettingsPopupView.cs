// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using TMPro;

public class SettingsPopupView : ViewBase
{
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private Button _backButton;

    private bool _isChangingLanguage;

    protected override void OnShow()
    {
        _backButton.onClick.AddListener(OnBackClicked);

        _musicToggle.isOn = AudioManager.Instance.IsMusicOn();
        _sfxToggle.isOn = AudioManager.Instance.IsSFXOn();
        _musicToggle.onValueChanged.AddListener(OnMusicToggled);
        _sfxToggle.onValueChanged.AddListener(OnSFXToggled);

        _languageDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            options.Add(locale.LocaleName);
        }

        _languageDropdown.AddOptions(options);

        _isChangingLanguage = true;

        for (var i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if (LocalizationSettings.AvailableLocales.Locales[i] == LocalizationSettings.SelectedLocale)
            {
                _languageDropdown.value = i;
                break;
            }
        }

        _languageDropdown.RefreshShownValue();
        _isChangingLanguage = false;

        _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    protected override void OnHide()
    {
        _backButton.onClick.RemoveAllListeners();
        _musicToggle.onValueChanged.RemoveAllListeners();
        _sfxToggle.onValueChanged.RemoveAllListeners();
        _languageDropdown.onValueChanged.RemoveAllListeners();
    }

    private void OnBackClicked()
    {
        ViewManager.Instance.ClosePopup<SettingsPopupView>();
    }

    private void OnMusicToggled(bool value)
    {
        AudioManager.Instance.SetMusicOn(value);
    }

    private void OnSFXToggled(bool value)
    {
        AudioManager.Instance.SetSFXOn(value);
    }

    private void OnLanguageChanged(int index)
    {
        if (_isChangingLanguage)
        {
            return;
        }

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}