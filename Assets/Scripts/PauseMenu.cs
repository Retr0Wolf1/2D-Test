using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseButton;
    public GameObject PausePanel;
    public Button OpenPauseButton;
    public Button ResumeButton;
    public Button MainMenuButton;

    private MainMenu m_MainMenu;

    private void Start()
    {
        m_MainMenu = FindObjectOfType<MainMenu>();

        if (OpenPauseButton != null)
            OpenPauseButton.onClick.AddListener(OpenPause);

        if (ResumeButton != null)
            ResumeButton.onClick.AddListener(Resume);

        if (MainMenuButton != null)
            MainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (PauseButton != null)
            PauseButton.SetActive(false);
    }

    public void ShowPauseButton()
    {
        if (PauseButton != null)
            PauseButton.SetActive(true);

        if (PausePanel != null)
            PausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenPause()
    {
        Time.timeScale = 0f;

        if (PauseButton != null)
            PauseButton.SetActive(false);

        if (PausePanel != null)
            PausePanel.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (PauseButton != null)
            PauseButton.SetActive(true);
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (m_MainMenu != null)
            m_MainMenu.ShowMenu();

        GameManager.Instance.ReturnToMainMenu();
    }
}