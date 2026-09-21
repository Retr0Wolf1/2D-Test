using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;

    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount = 100;
    private int m_CurrentLevel = 1;
    private bool m_IsGameOver = false;

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

        PlayerController.gameObject.SetActive(false);
    }

    void OnTurnHappen() => ChangeFood(-1);

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        UIManager.Instance.UpdateFood(m_FoodAmount);

        SaveGame();

        if (m_FoodAmount <= 0)
        {
            m_IsGameOver = true;
            SaveManager.TrySaveBestLevel(m_CurrentLevel);
            SaveManager.ClearSave();

            PlayerController.GameOver();
            UIManager.Instance.ShowGameOver();
        }
    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        SaveManager.TrySaveBestLevel(m_CurrentLevel);

        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        SaveGame();
    }

    public void StartNewGame()
    {
        m_IsGameOver = false;

        m_CurrentLevel = 1;
        m_FoodAmount = 20;
        UIManager.Instance.UpdateFood(m_FoodAmount);
        UIManager.Instance.ShowFoodLabel(true);

        BoardManager.Seed = 0;

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.gameObject.SetActive(true);
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        SaveGame();
    }

    public void ContinueGame()
    {
        if (!SaveManager.HasSave) return;

        m_IsGameOver = false;

        m_CurrentLevel = SaveManager.LoadLevel();
        m_FoodAmount = SaveManager.LoadFood();
        UIManager.Instance.UpdateFood(m_FoodAmount);
        UIManager.Instance.ShowFoodLabel(true);

        BoardManager.Seed = SaveManager.LoadSeed();

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.gameObject.SetActive(true);
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, SaveManager.LoadPlayerPos());
    }

    void SaveGame()
    {
        if (BoardManager == null || PlayerController == null) return;
        if (m_IsGameOver) return;

        SaveManager.SaveGame(
            BoardManager.Seed,
            m_FoodAmount,
            m_CurrentLevel,
            PlayerController.Cell
        );
    }

    public void ReturnToMainMenu()
    {
        if (!m_IsGameOver)
            SaveGame();

        BoardManager.Clean();
        UIManager.Instance.ShowFoodLabel(false);
        PlayerController.gameObject.SetActive(false);
    }

    public void ClearSave()
    {
        SaveManager.ClearSave();
    }
}