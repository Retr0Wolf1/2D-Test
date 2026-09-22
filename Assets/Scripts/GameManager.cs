// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.
    
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [FormerlySerializedAs("BoardManager")]
    [SerializeField] private BoardManager _boardManager;

    [FormerlySerializedAs("PlayerController")]
    [SerializeField] private PlayerController _playerController;

    private int _foodAmount = 100;
    private int _currentLevel = 1;
    private bool _isGameOver;

    public BoardManager BoardManager => _boardManager;
    public PlayerController PlayerController => _playerController;
    public TurnManager TurnManager { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        _playerController.gameObject.SetActive(false);
    }

    public void ChangeFood(int amount)
    {
        _foodAmount += amount;

        HudPageView hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.UpdateFood(_foodAmount);
        }

        SaveGame();

        if (_foodAmount <= 0)
        {
            _isGameOver = true;
            SaveManager.TrySaveBestLevel(_currentLevel);
            SaveManager.ClearSave();

            _playerController.GameOver();
            ViewManager.Instance.OpenPopup<GameOverPopupView>();
        }
    }

    public void NewLevel()
    {
        _currentLevel++;
        SaveManager.TrySaveBestLevel(_currentLevel);

        _boardManager.Clean();
        _boardManager.Init();
        _playerController.Spawn(_boardManager, new Vector2Int(1, 1));

        ViewManager.Instance.OpenPage<HudPageView>();
        SaveGame();
    }

    public void StartNewGame()
    {
        _isGameOver = false;
        _currentLevel = 1;
        _foodAmount = 20;

        _boardManager.Seed = 0;
        _boardManager.Clean();
        _boardManager.Init();

        _playerController.gameObject.SetActive(true);
        _playerController.Init();
        _playerController.Spawn(_boardManager, new Vector2Int(1, 1));

        ViewManager.Instance.OpenPage<HudPageView>();

        HudPageView hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.ShowHint();
        }

        SaveGame();
    }

    public void ContinueGame()
    {
        if (!SaveManager.HasSave)
        {
            return;
        }

        _isGameOver = false;
        _currentLevel = SaveManager.LoadLevel();
        _foodAmount = SaveManager.LoadFood();
        _boardManager.Seed = SaveManager.LoadSeed();

        _boardManager.Clean();
        _boardManager.Init();

        _playerController.gameObject.SetActive(true);
        _playerController.Init();
        _playerController.Spawn(_boardManager, SaveManager.LoadPlayerPos());

        ViewManager.Instance.OpenPage<HudPageView>();
    }

    public void ReturnToMainMenu()
    {
        if (!_isGameOver)
        {
            SaveGame();
        }

        _boardManager.Clean();
        _playerController.gameObject.SetActive(false);
    }

    public void ClearSave()
    {
        SaveManager.ClearSave();
    }

    private void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    private void SaveGame()
    {
        if (_boardManager == null || _playerController == null)
        {
            return;
        }

        if (_isGameOver)
        {
            return;
        }

        SaveManager.SaveGame(
            _boardManager.Seed,
            _foodAmount,
            _currentLevel,
            _playerController.Cell
        );
    }
}