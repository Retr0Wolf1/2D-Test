// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class SettingsPopupView : ViewBase
{
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _settingsTitle;

    private bool _isChangingLanguage;

    protected override void OnShow()
    {
        _settingsTitle.text = GetText("settings_button");
        SetButtonText(_backButton, "back_button");

        SetToggleText(_musicToggle, "music_label");
        SetToggleText(_sfxToggle, "sounds_label");

        _backButton.onClick.AddListener(OnBackClicked);

        _musicToggle.isOn = AudioManager.Instance.IsMusicOn();
        _sfxToggle.isOn = AudioManager.Instance.IsSFXOn();
        _musicToggle.onValueChanged.AddListener(OnMusicToggled);
        _sfxToggle.onValueChanged.AddListener(OnSFXToggled);

        _languageDropdown.ClearOptions();
        _languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "Русский" });

        string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        _isChangingLanguage = true;
        _languageDropdown.value = currentCode.StartsWith("ru") ? 1 : 0;
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

    private void SetToggleText(Toggle toggle, string key)
    {
        if (toggle == null)
        {
            return;
        }

        TextMeshProUGUI text = toggle.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = GetText(key);
        }
    }

    private string GetText(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", key);
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

        string localeCode = index == 0 ? "en" : "ru";
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale == null)
        {
            foreach (var availableLocale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (availableLocale.Identifier.Code.StartsWith(localeCode))
                {
                    locale = availableLocale;
                    break;
                }
            }
        }

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}