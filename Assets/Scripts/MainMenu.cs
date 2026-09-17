using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject MenuPanel;
    public Button StartButton;
    public Button QuitButton;

    private void Start()
    {
        if (StartButton != null)
            StartButton.onClick.AddListener(OnStartClicked);

        if (QuitButton != null)
            QuitButton.onClick.AddListener(OnQuitClicked);

        ShowMenu();
    }

    public void ShowMenu()
    {
        if (MenuPanel != null)
            MenuPanel.SetActive(true);
    }

    public void HideMenu()
    {
        if (MenuPanel != null)
            MenuPanel.SetActive(false);
    }

    void OnStartClicked()
    {
        HideMenu();
        GameManager.Instance.StartNewGame();
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}