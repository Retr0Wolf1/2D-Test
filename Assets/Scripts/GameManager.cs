using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public PauseMenu PauseMenu;
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount = 100;
    private int m_CurrentLevel = 1;
    private Label m_FoodLabel;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

        m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_FoodLabel.style.visibility = Visibility.Hidden;

        PlayerController.gameObject.SetActive(false);
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " days\n\nPress Enter to restart";
        }
    }

    public void NewLevel()
    {
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        m_CurrentLevel++;

        if (Camera.main != null)
        {
            CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
            if (follow != null)
                follow.SetTarget(PlayerController.transform);
        }
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_FoodLabel.style.visibility = Visibility.Visible;

        m_CurrentLevel = 1;
        m_FoodAmount = 20;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.gameObject.SetActive(true);
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        if (Camera.main != null)
        {
            CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
            if (follow != null)
                follow.SetTarget(PlayerController.transform);
        }

        if (PauseMenu != null)
            PauseMenu.ShowPauseButton();
    }

    public void ReturnToMainMenu()
    {
        BoardManager.Clean();

        m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_FoodLabel.style.visibility = Visibility.Hidden;

        PlayerController.gameObject.SetActive(false);
    }
}