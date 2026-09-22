// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainPageView : ViewBase
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private TextMeshProUGUI _titleText;

    protected override void OnShow()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _continueButton.onClick.AddListener(OnContinueClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);

        _continueButton.gameObject.SetActive(SaveManager.HasSave);
        _bestScoreText.text = LocalizationManager.Get("best") + ": " + SaveManager.BestLevel + " " + LocalizationManager.Get("days");
        _titleText.text = "Roguelike";
    }

    protected override void OnHide()
    {
        _startButton.onClick.RemoveAllListeners();
        _continueButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
    }

    private void OnStartClicked() => GameManager.Instance.StartNewGame();
    private void OnContinueClicked() => GameManager.Instance.ContinueGame();
    private void OnSettingsClicked() => ViewManager.Instance.OpenPopup<SettingsPopupView>();

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}