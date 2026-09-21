using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject HUDPanel;
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public GameObject SettingsPanel;

    [Header("Main Menu")]
    public Button StartButton;
    public Button ContinueButton;
    public Button QuitButton;
    public Button SettingsButton;
    public TextMeshProUGUI BestScoreText;
    public TextMeshProUGUI TitleText;

    [Header("HUD")]
    public Button PauseButton;
    public TextMeshProUGUI FoodLabel;
    public TextMeshProUGUI GoToExitText;
    public TextMeshProUGUI HintText;
    public Color NormalFoodColor = Color.white;
    public Color LowFoodColor = Color.red;
    public int LowFoodThreshold = 5;
    public float BlinkSpeed = 2f;

    public Color GoToExitColorA = Color.white;
    public Color GoToExitColorB = Color.yellow;
    public float GoToExitBlinkSpeed = 2f;

    public float HintCharDelay = 0.03f;
    public float HintDuration = 5f;

    [Header("Pause")]
    public Button ResumeButton;
    public Button PauseMainMenuButton;
    public Button PauseSettingsButton;
    public TextMeshProUGUI PauseTitle;

    [Header("Settings")]
    public Toggle MusicToggle;
    public Toggle SFXToggle;
    public Button SettingsBackButton;
    public TMP_Dropdown LanguageDropdown;
    public TextMeshProUGUI SettingsTitle;

    [Header("Game Over")]
    public Button RestartButton;
    public Button GameOverMainMenuButton;
    public TextMeshProUGUI GameOverTitle;
    public TextMeshProUGUI GameOverBestText;

    private GameObject m_PreviousPanel;
    private bool m_IsLowFood = false;
    private bool m_IsGoToExitVisible = false;
    private bool m_IsChangingLanguage = false;
    private Coroutine m_HintCoroutine;
    private bool m_HintVisible = false;
    private bool m_HintTyping = false;

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
        if (StartButton != null) StartButton.onClick.AddListener(OnStartClicked);
        if (ContinueButton != null) ContinueButton.onClick.AddListener(OnContinueClicked);
        if (QuitButton != null) QuitButton.onClick.AddListener(OnQuitClicked);
        if (SettingsButton != null) SettingsButton.onClick.AddListener(OnSettingsClicked);
        if (PauseButton != null) PauseButton.onClick.AddListener(OnPauseClicked);
        if (ResumeButton != null) ResumeButton.onClick.AddListener(OnResumeClicked);
        if (PauseMainMenuButton != null) PauseMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (PauseSettingsButton != null) PauseSettingsButton.onClick.AddListener(OnSettingsClicked);
        if (SettingsBackButton != null) SettingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        if (RestartButton != null) RestartButton.onClick.AddListener(OnRestartClicked);
        if (GameOverMainMenuButton != null) GameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (LanguageDropdown != null)
        {
            LanguageDropdown.ClearOptions();
            LanguageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "Русский" });
            LanguageDropdown.value = (int)LocalizationManager.Current;
            LanguageDropdown.RefreshShownValue();
            LanguageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (MusicToggle != null)
        {
            MusicToggle.isOn = AudioManager.Instance.IsMusicOn();
            MusicToggle.onValueChanged.AddListener(OnMusicToggled);
        }

        if (SFXToggle != null)
        {
            SFXToggle.isOn = AudioManager.Instance.IsSFXOn();
            SFXToggle.onValueChanged.AddListener(OnSFXToggled);
        }

        LocalizationManager.OnLanguageChanged += RefreshUI;
        RefreshUI();
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        SetButtonText(StartButton, "start");
        SetButtonText(ContinueButton, "continue");
        SetButtonText(QuitButton, "quit");
        SetButtonText(SettingsButton, "settings");
        SetButtonText(ResumeButton, "resume");
        SetButtonText(PauseMainMenuButton, "mainmenu");
        SetButtonText(PauseSettingsButton, "settings");
        SetButtonText(SettingsBackButton, "back");
        SetButtonText(RestartButton, "restart");
        SetButtonText(GameOverMainMenuButton, "mainmenu");

        if (TitleText != null)
            TitleText.text = "Roguelike";

        if (SettingsTitle != null)
            SettingsTitle.text = LocalizationManager.Get("settings");

        if (PauseTitle != null)
            PauseTitle.text = LocalizationManager.Get("paused");

        if (GameOverTitle != null)
            GameOverTitle.text = LocalizationManager.Get("gameover");

        if (GoToExitText != null)
            GoToExitText.text = LocalizationManager.Get("goexit");

        if (MusicToggle != null)
        {
            TextMeshProUGUI lbl = MusicToggle.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = LocalizationManager.Get("music");
        }

        if (SFXToggle != null)
        {
            TextMeshProUGUI lbl = SFXToggle.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = LocalizationManager.Get("sounds");
        }

        if (LanguageDropdown != null)
        {
            m_IsChangingLanguage = true;
            LanguageDropdown.value = (int)LocalizationManager.Current;
            LanguageDropdown.RefreshShownValue();
            m_IsChangingLanguage = false;
        }
    }

    void SetButtonText(Button button, string key)
    {
        if (button == null) return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = LocalizationManager.Get(key);
    }

    private void Update()
    {
        if (FoodLabel != null && m_IsLowFood)
        {
            float t = Mathf.PingPong(Time.unscaledTime * BlinkSpeed, 1f);
            FoodLabel.color = Color.Lerp(NormalFoodColor, LowFoodColor, t);
        }

        if (GoToExitText != null && m_IsGoToExitVisible)
        {
            float t = Mathf.PingPong(Time.unscaledTime * GoToExitBlinkSpeed, 1f);
            GoToExitText.color = Color.Lerp(GoToExitColorA, GoToExitColorB, t);
        }

        if (m_HintVisible)
        {
            bool skip = false;

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                skip = true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                skip = true;

            if (skip)
                SkipHint();
        }
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        SetActive(MainMenuPanel, true);
        SetActive(HUDPanel, false);
        SetActive(PausePanel, false);
        SetActive(GameOverPanel, false);
        SetActive(SettingsPanel, false);
        m_PreviousPanel = MainMenuPanel;

        HideHint();

        if (ContinueButton != null)
            ContinueButton.gameObject.SetActive(SaveManager.HasSave);

        if (BestScoreText != null)
            BestScoreText.text = LocalizationManager.Get("best") + ": " + SaveManager.BestLevel + " " + LocalizationManager.Get("days");
    }

    public void ShowHUD(bool showHint = false)
    {
        Time.timeScale = 1f;
        SetActive(MainMenuPanel, false);
        SetActive(HUDPanel, true);
        SetActive(PausePanel, false);
        SetActive(GameOverPanel, false);
        SetActive(SettingsPanel, false);

        m_IsGoToExitVisible = false;

        if (GoToExitText != null)
            GoToExitText.gameObject.SetActive(false);

        if (showHint)
            ShowHint();
        else
            HideHint();
    }

    public void ShowPause()
    {
        Time.timeScale = 0f;
        SetActive(MainMenuPanel, false);
        SetActive(HUDPanel, true);
        SetActive(PausePanel, true);
        SetActive(GameOverPanel, false);
        SetActive(SettingsPanel, false);
        m_PreviousPanel = PausePanel;

        if (PauseTitle != null)
            PauseTitle.text = LocalizationManager.Get("paused");
    }

    public void ShowGameOver()
    {
        Time.timeScale = 1f;
        SetActive(MainMenuPanel, false);
        SetActive(HUDPanel, false);
        SetActive(PausePanel, false);
        SetActive(GameOverPanel, true);
        SetActive(SettingsPanel, false);
        m_PreviousPanel = GameOverPanel;

        if (GameOverBestText != null)
            GameOverBestText.text = LocalizationManager.Get("best") + ": " + SaveManager.BestLevel + " " + LocalizationManager.Get("days");

        if (GameOverTitle != null)
            GameOverTitle.text = LocalizationManager.Get("gameover");
    }

    public void ShowSettings()
    {
        SetActive(MainMenuPanel, false);
        SetActive(HUDPanel, false);
        SetActive(PausePanel, false);
        SetActive(GameOverPanel, false);
        SetActive(SettingsPanel, true);

        if (MusicToggle != null) MusicToggle.isOn = AudioManager.Instance.IsMusicOn();
        if (SFXToggle != null) SFXToggle.isOn = AudioManager.Instance.IsSFXOn();

        if (SettingsTitle != null)
            SettingsTitle.text = LocalizationManager.Get("settings");
    }

    public void ShowGoToExit()
    {
        if (GoToExitText == null) return;

        m_IsGoToExitVisible = true;
        GoToExitText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideGoToExit));
        Invoke(nameof(HideGoToExit), 3f);
    }

    void HideGoToExit()
    {
        m_IsGoToExitVisible = false;

        if (GoToExitText != null)
            GoToExitText.gameObject.SetActive(false);
    }

    public void ShowHint()
    {
        if (HintText == null) return;

        if (m_HintCoroutine != null)
            StopCoroutine(m_HintCoroutine);

        m_HintCoroutine = StartCoroutine(TypeHint());
    }

    private System.Collections.IEnumerator TypeHint()
    {
        HintText.text = "";
        HintText.gameObject.SetActive(true);
        HintText.raycastTarget = false;

        m_HintVisible = true;
        m_HintTyping = true;

        string full = LocalizationManager.Get("hint");

        for (int i = 0; i <= full.Length; i++)
        {
            if (!m_HintTyping)
            {
                HintText.text = full;
                break;
            }

            HintText.text = full.Substring(0, i);
            yield return new WaitForSecondsRealtime(HintCharDelay);
        }

        m_HintTyping = false;

        yield return new WaitForSecondsRealtime(HintDuration);

        HideHint();
    }

    public void SkipHint()
    {
        if (!m_HintVisible) return;

        if (m_HintTyping)
        {
            m_HintTyping = false;
            if (HintText != null)
                HintText.text = LocalizationManager.Get("hint");
        }
        else
        {
            HideHint();
        }
    }

    void HideHint()
    {
        m_HintVisible = false;
        m_HintTyping = false;

        if (m_HintCoroutine != null)
        {
            StopCoroutine(m_HintCoroutine);
            m_HintCoroutine = null;
        }

        if (HintText != null)
            HintText.gameObject.SetActive(false);
    }

    public void UpdateFood(int amount)
    {
        if (FoodLabel == null) return;

        FoodLabel.text = LocalizationManager.Get("food") + " : " + amount;

        if (amount <= LowFoodThreshold)
            m_IsLowFood = true;
        else
        {
            m_IsLowFood = false;
            FoodLabel.color = NormalFoodColor;
        }
    }

    public void ShowFoodLabel(bool value)
    {
        if (FoodLabel != null)
            FoodLabel.gameObject.SetActive(value);
    }

    void OnStartClicked()
    {
        ShowHUD(true);
        GameManager.Instance.StartNewGame();
    }

    void OnContinueClicked()
    {
        ShowHUD(false);
        GameManager.Instance.ContinueGame();
    }

    void OnPauseClicked() => ShowPause();
    void OnResumeClicked() => ShowHUD(false);

    void OnSettingsClicked()
    {
        if (PausePanel != null && PausePanel.activeSelf) m_PreviousPanel = PausePanel;
        else if (GameOverPanel != null && GameOverPanel.activeSelf) m_PreviousPanel = GameOverPanel;
        else m_PreviousPanel = MainMenuPanel;
        ShowSettings();
    }

    void OnSettingsBackClicked()
    {
        if (m_PreviousPanel == PausePanel) ShowPause();
        else if (m_PreviousPanel == GameOverPanel) ShowGameOver();
        else ShowMainMenu();
    }

    void OnLanguageChanged(int index)
    {
        if (m_IsChangingLanguage) return;

        LocalizationManager.Current = (LocalizationManager.Language)index;
    }

    void OnMusicToggled(bool value) => AudioManager.Instance.SetMusicOn(value);
    void OnSFXToggled(bool value) => AudioManager.Instance.SetSFXOn(value);

    void OnRestartClicked()
    {
        ShowHUD(false);
        GameManager.Instance.StartNewGame();
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToMainMenu();
        ShowMainMenu();
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetActive(GameObject panel, bool value)
    {
        if (panel != null)
            panel.SetActive(value);
    }
}