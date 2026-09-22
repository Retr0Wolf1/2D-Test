// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [FormerlySerializedAs("MainMenuPanel")]
    [SerializeField] private GameObject _mainMenuPanel;

    [FormerlySerializedAs("HUDPanel")]
    [SerializeField] private GameObject _hudPanel;

    [FormerlySerializedAs("PausePanel")]
    [SerializeField] private GameObject _pausePanel;

    [FormerlySerializedAs("GameOverPanel")]
    [SerializeField] private GameObject _gameOverPanel;

    [FormerlySerializedAs("SettingsPanel")]
    [SerializeField] private GameObject _settingsPanel;

    [Header("Main Menu")]
    [FormerlySerializedAs("StartButton")]
    [SerializeField] private Button _startButton;

    [FormerlySerializedAs("ContinueButton")]
    [SerializeField] private Button _continueButton;

    [FormerlySerializedAs("QuitButton")]
    [SerializeField] private Button _quitButton;

    [FormerlySerializedAs("SettingsButton")]
    [SerializeField] private Button _settingsButton;

    [FormerlySerializedAs("BestScoreText")]
    [SerializeField] private TextMeshProUGUI _bestScoreText;

    [FormerlySerializedAs("TitleText")]
    [SerializeField] private TextMeshProUGUI _titleText;

    [Header("HUD")]
    [FormerlySerializedAs("PauseButton")]
    [SerializeField] private Button _pauseButton;

    [FormerlySerializedAs("FoodLabel")]
    [SerializeField] private TextMeshProUGUI _foodLabel;

    [FormerlySerializedAs("GoToExitText")]
    [SerializeField] private TextMeshProUGUI _goToExitText;

    [FormerlySerializedAs("HintText")]
    [SerializeField] private TextMeshProUGUI _hintText;

    [FormerlySerializedAs("NormalFoodColor")]
    [SerializeField] private Color _normalFoodColor = Color.white;

    [FormerlySerializedAs("LowFoodColor")]
    [SerializeField] private Color _lowFoodColor = Color.red;

    [FormerlySerializedAs("LowFoodThreshold")]
    [SerializeField] private int _lowFoodThreshold = 5;

    [FormerlySerializedAs("BlinkSpeed")]
    [SerializeField] private float _blinkSpeed = 2f;

    [FormerlySerializedAs("GoToExitColorA")]
    [SerializeField] private Color _goToExitColorA = Color.white;

    [FormerlySerializedAs("GoToExitColorB")]
    [SerializeField] private Color _goToExitColorB = Color.yellow;

    [FormerlySerializedAs("GoToExitBlinkSpeed")]
    [SerializeField] private float _goToExitBlinkSpeed = 2f;

    [FormerlySerializedAs("HintCharDelay")]
    [SerializeField] private float _hintCharDelay = 0.03f;

    [FormerlySerializedAs("HintDuration")]
    [SerializeField] private float _hintDuration = 5f;

    [Header("Pause")]
    [FormerlySerializedAs("ResumeButton")]
    [SerializeField] private Button _resumeButton;

    [FormerlySerializedAs("PauseMainMenuButton")]
    [SerializeField] private Button _pauseMainMenuButton;

    [FormerlySerializedAs("PauseSettingsButton")]
    [SerializeField] private Button _pauseSettingsButton;

    [FormerlySerializedAs("PauseTitle")]
    [SerializeField] private TextMeshProUGUI _pauseTitle;

    [Header("Settings")]
    [FormerlySerializedAs("MusicToggle")]
    [SerializeField] private Toggle _musicToggle;

    [FormerlySerializedAs("SFXToggle")]
    [SerializeField] private Toggle _sfxToggle;

    [FormerlySerializedAs("SettingsBackButton")]
    [SerializeField] private Button _settingsBackButton;

    [FormerlySerializedAs("LanguageDropdown")]
    [SerializeField] private TMP_Dropdown _languageDropdown;

    [FormerlySerializedAs("SettingsTitle")]
    [SerializeField] private TextMeshProUGUI _settingsTitle;

    [Header("Game Over")]
    [FormerlySerializedAs("RestartButton")]
    [SerializeField] private Button _restartButton;

    [FormerlySerializedAs("GameOverMainMenuButton")]
    [SerializeField] private Button _gameOverMainMenuButton;

    [FormerlySerializedAs("GameOverTitle")]
    [SerializeField] private TextMeshProUGUI _gameOverTitle;

    [FormerlySerializedAs("GameOverBestText")]
    [SerializeField] private TextMeshProUGUI _gameOverBestText;

    private GameObject _previousPanel;
    private bool _isLowFood;
    private bool _isGoToExitVisible;
    private bool _isChangingLanguage;
    private Coroutine _hintCoroutine;
    private bool _hintVisible;
    private bool _hintTyping;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (_startButton != null) _startButton.onClick.AddListener(OnStartClicked);
        if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);
        if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
        if (_pauseButton != null) _pauseButton.onClick.AddListener(OnPauseClicked);
        if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumeClicked);
        if (_pauseMainMenuButton != null) _pauseMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (_pauseSettingsButton != null) _pauseSettingsButton.onClick.AddListener(OnSettingsClicked);
        if (_settingsBackButton != null) _settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        if (_restartButton != null) _restartButton.onClick.AddListener(OnRestartClicked);
        if (_gameOverMainMenuButton != null) _gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (_languageDropdown != null)
        {
            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "Русский" });
            _languageDropdown.value = (int)LocalizationManager.Current;
            _languageDropdown.RefreshShownValue();
            _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (_musicToggle != null)
        {
            _musicToggle.isOn = AudioManager.Instance.IsMusicOn();
            _musicToggle.onValueChanged.AddListener(OnMusicToggled);
        }

        if (_sfxToggle != null)
        {
            _sfxToggle.isOn = AudioManager.Instance.IsSFXOn();
            _sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        }

        LocalizationManager.OnLanguageChanged += RefreshUI;
        RefreshUI();
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshUI;
    }

    private void Update()
    {
        if (_foodLabel != null && _isLowFood)
        {
            float t = Mathf.PingPong(Time.unscaledTime * _blinkSpeed, 1f);
            _foodLabel.color = Color.Lerp(_normalFoodColor, _lowFoodColor, t);
        }

        if (_goToExitText != null && _isGoToExitVisible)
        {
            float t = Mathf.PingPong(Time.unscaledTime * _goToExitBlinkSpeed, 1f);
            _goToExitText.color = Color.Lerp(_goToExitColorA, _goToExitColorB, t);
        }

        if (_hintVisible)
        {
            bool skip = false;

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                skip = true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                skip = true;
            }

            if (skip)
            {
                SkipHint();
            }
        }
    }

    public void RefreshUI()
    {
        SetButtonText(_startButton, "start");
        SetButtonText(_continueButton, "continue");
        SetButtonText(_quitButton, "quit");
        SetButtonText(_settingsButton, "settings");
        SetButtonText(_resumeButton, "resume");
        SetButtonText(_pauseMainMenuButton, "mainmenu");
        SetButtonText(_pauseSettingsButton, "settings");
        SetButtonText(_settingsBackButton, "back");
        SetButtonText(_restartButton, "restart");
        SetButtonText(_gameOverMainMenuButton, "mainmenu");

        if (_titleText != null)
        {
            _titleText.text = "Roguelike";
        }

        if (_settingsTitle != null)
        {
            _settingsTitle.text = LocalizationManager.Get("settings");
        }

        if (_pauseTitle != null)
        {
            _pauseTitle.text = LocalizationManager.Get("paused");
        }

        if (_gameOverTitle != null)
        {
            _gameOverTitle.text = LocalizationManager.Get("gameover");
        }

        if (_goToExitText != null)
        {
            _goToExitText.text = LocalizationManager.Get("goexit");
        }

        if (_musicToggle != null)
        {
            TextMeshProUGUI label = _musicToggle.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                label.text = LocalizationManager.Get("music");
            }
        }

        if (_sfxToggle != null)
        {
            TextMeshProUGUI label = _sfxToggle.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                label.text = LocalizationManager.Get("sounds");
            }
        }

        if (_languageDropdown != null)
        {
            _isChangingLanguage = true;
            _languageDropdown.value = (int)LocalizationManager.Current;
            _languageDropdown.RefreshShownValue();
            _isChangingLanguage = false;
        }
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        SetActive(_mainMenuPanel, true);
        SetActive(_hudPanel, false);
        SetActive(_pausePanel, false);
        SetActive(_gameOverPanel, false);
        SetActive(_settingsPanel, false);

        _previousPanel = _mainMenuPanel;

        ShowFoodLabel(false);
        HideGoToExit();
        HideHint();

        if (_continueButton != null)
        {
            _continueButton.gameObject.SetActive(SaveManager.HasSave);
        }

        if (_bestScoreText != null)
        {
            _bestScoreText.text = LocalizationManager.Get("best") + ": " + SaveManager.BestLevel + " " + LocalizationManager.Get("days");
        }
    }

    public void ShowHUD(bool showHint = false)
    {
        Time.timeScale = 1f;
        SetActive(_mainMenuPanel, false);
        SetActive(_hudPanel, true);
        SetActive(_pausePanel, false);
        SetActive(_gameOverPanel, false);
        SetActive(_settingsPanel, false);

        ShowFoodLabel(true);
        _isGoToExitVisible = false;

        if (_goToExitText != null)
        {
            _goToExitText.gameObject.SetActive(false);
        }

        if (showHint)
        {
            ShowHint();
        }
        else
        {
            HideHint();
        }
    }

    public void ShowPause()
    {
        Time.timeScale = 0f;
        SetActive(_mainMenuPanel, false);
        SetActive(_hudPanel, true);
        SetActive(_pausePanel, true);
        SetActive(_gameOverPanel, false);
        SetActive(_settingsPanel, false);

        _previousPanel = _pausePanel;

        ShowFoodLabel(true);
        HideGoToExit();
        HideHint();

        if (_pauseTitle != null)
        {
            _pauseTitle.text = LocalizationManager.Get("paused");
        }
    }

    public void ShowGameOver()
    {
        Time.timeScale = 1f;
        SetActive(_mainMenuPanel, false);
        SetActive(_hudPanel, false);
        SetActive(_pausePanel, false);
        SetActive(_gameOverPanel, true);
        SetActive(_settingsPanel, false);

        _previousPanel = _gameOverPanel;

        ShowFoodLabel(false);
        HideGoToExit();
        HideHint();

        if (_gameOverBestText != null)
        {
            _gameOverBestText.text = LocalizationManager.Get("best") + ": " + SaveManager.BestLevel + " " + LocalizationManager.Get("days");
        }

        if (_gameOverTitle != null)
        {
            _gameOverTitle.text = LocalizationManager.Get("gameover");
        }
    }

    public void ShowSettings()
    {
        SetActive(_mainMenuPanel, false);
        SetActive(_hudPanel, false);
        SetActive(_pausePanel, false);
        SetActive(_gameOverPanel, false);
        SetActive(_settingsPanel, true);

        ShowFoodLabel(false);
        HideGoToExit();
        HideHint();

        if (_musicToggle != null)
        {
            _musicToggle.isOn = AudioManager.Instance.IsMusicOn();
        }

        if (_sfxToggle != null)
        {
            _sfxToggle.isOn = AudioManager.Instance.IsSFXOn();
        }

        if (_settingsTitle != null)
        {
            _settingsTitle.text = LocalizationManager.Get("settings");
        }
    }

    public void ShowGoToExit()
    {
        if (_goToExitText == null)
        {
            return;
        }

        _isGoToExitVisible = true;
        _goToExitText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideGoToExit));
        Invoke(nameof(HideGoToExit), 3f);
    }

    public void ShowHint()
    {
        if (_hintText == null)
        {
            return;
        }

        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
        }

        _hintCoroutine = StartCoroutine(TypeHint());
    }

    public void SkipHint()
    {
        if (!_hintVisible)
        {
            return;
        }

        if (_hintTyping)
        {
            _hintTyping = false;

            if (_hintText != null)
            {
                _hintText.text = LocalizationManager.Get("hint");
            }
        }
        else
        {
            HideHint();
        }
    }

    public void UpdateFood(int amount)
    {
        if (_foodLabel == null)
        {
            return;
        }

        _foodLabel.text = LocalizationManager.Get("food") + " : " + amount;

        if (amount <= _lowFoodThreshold)
        {
            _isLowFood = true;
        }
        else
        {
            _isLowFood = false;
            _foodLabel.color = _normalFoodColor;
        }
    }

    public void ShowFoodLabel(bool value)
    {
        if (_foodLabel != null)
        {
            _foodLabel.gameObject.SetActive(value);
        }
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
            text.text = LocalizationManager.Get(key);
        }
    }

    private void HideGoToExit()
    {
        _isGoToExitVisible = false;

        if (_goToExitText != null)
        {
            _goToExitText.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator TypeHint()
    {
        _hintText.text = "";
        _hintText.gameObject.SetActive(true);
        _hintText.raycastTarget = false;

        _hintVisible = true;
        _hintTyping = true;

        string full = LocalizationManager.Get("hint");

        for (int i = 0; i <= full.Length; i++)
        {
            if (!_hintTyping)
            {
                _hintText.text = full;
                break;
            }

            _hintText.text = full.Substring(0, i);
            yield return new WaitForSecondsRealtime(_hintCharDelay);
        }

        _hintTyping = false;

        yield return new WaitForSecondsRealtime(_hintDuration);

        HideHint();
    }

    private void HideHint()
    {
        _hintVisible = false;
        _hintTyping = false;

        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
            _hintCoroutine = null;
        }

        if (_hintText != null)
        {
            _hintText.gameObject.SetActive(false);
        }
    }

    private void OnStartClicked()
    {
        ShowHUD(true);
        GameManager.Instance.StartNewGame();
    }

    private void OnContinueClicked()
    {
        ShowHUD(false);
        GameManager.Instance.ContinueGame();
    }

    private void OnPauseClicked()
    {
        ShowPause();
    }

    private void OnResumeClicked()
    {
        ShowHUD(false);
    }

    private void OnSettingsClicked()
    {
        if (_pausePanel != null && _pausePanel.activeSelf)
        {
            _previousPanel = _pausePanel;
        }
        else if (_gameOverPanel != null && _gameOverPanel.activeSelf)
        {
            _previousPanel = _gameOverPanel;
        }
        else
        {
            _previousPanel = _mainMenuPanel;
        }

        ShowSettings();
    }

    private void OnSettingsBackClicked()
    {
        if (_previousPanel == _pausePanel)
        {
            ShowPause();
        }
        else if (_previousPanel == _gameOverPanel)
        {
            ShowGameOver();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void OnLanguageChanged(int index)
    {
        if (_isChangingLanguage)
        {
            return;
        }

        LocalizationManager.Current = (LocalizationManager.Language)index;
    }

    private void OnMusicToggled(bool value)
    {
        AudioManager.Instance.SetMusicOn(value);
    }

    private void OnSFXToggled(bool value)
    {
        AudioManager.Instance.SetSFXOn(value);
    }

    private void OnRestartClicked()
    {
        ShowHUD(false);
        GameManager.Instance.StartNewGame();
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToMainMenu();
        ShowMainMenu();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetActive(GameObject panel, bool value)
    {
        if (panel != null)
        {
            panel.SetActive(value);
        }
    }
}