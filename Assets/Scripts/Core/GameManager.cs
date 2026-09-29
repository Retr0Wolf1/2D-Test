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

    [Header("Revive")]
    [SerializeField] private int _maxRevivesPerRun = 3;
    [SerializeField] private int _reviveFood = 20;

    private int _foodAmount = 100;
    private int _currentLevel = 1;
    private bool _isGameOver;
    private int _revivesUsed;

    public BoardManager BoardManager => _boardManager;
    public PlayerController PlayerController => _playerController;
    public TurnManager TurnManager { get; private set; }
    public int CurrentLevel => _currentLevel;
    public int RevivesLeft => Mathf.Max(0, _maxRevivesPerRun - _revivesUsed);

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

        var hud = ViewManager.Instance.CurrentPage as HudPageView;

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

            AnalyticsManager.RunEnded(_currentLevel, SaveManager.GetBestLevel());

            _playerController.GameOver();
            ViewManager.Instance.OpenPopup<GameOverPopupView>();
        }
    }

    // Лимит за катку есть только у воскрешений за рекламу, платные не ограничены.
    public void Revive(bool isFromAd)
    {
        if (!_isGameOver || (isFromAd && RevivesLeft <= 0))
        {
            return;
        }

        if (isFromAd)
        {
            _revivesUsed++;
        }

        _isGameOver = false;
        _foodAmount = _reviveFood;

        _playerController.Init();

        ViewManager.Instance.ClosePopup<GameOverPopupView>();

        var hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.UpdateFood(_foodAmount);
        }

        SaveGame();

        AnalyticsManager.PlayerRevived(_currentLevel, isFromAd);
    }

    public void NewLevel()
    {
        AnalyticsManager.LevelCompleted(_currentLevel, _foodAmount);

        _currentLevel++;
        SaveManager.TrySaveBestLevel(_currentLevel);

        _boardManager.Seed = (int)System.DateTime.Now.Ticks;

        _boardManager.Clean();
        _boardManager.Init();
        _playerController.Spawn(_boardManager, new Vector2Int(1, 1));

        ViewManager.Instance.OpenPage<HudPageView>();

        var hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.UpdateFood(_foodAmount);
        }

        SaveGame();
    }

    public void StartNewGame()
    {
        _isGameOver = false;
        _currentLevel = 1;
        _foodAmount = 20;
        _revivesUsed = 0;

        _boardManager.Seed = (int)System.DateTime.Now.Ticks;

        _boardManager.Clean();
        _boardManager.Init();

        _playerController.gameObject.SetActive(true);
        _playerController.Init();
        _playerController.Spawn(_boardManager, new Vector2Int(1, 1));

        ViewManager.Instance.OpenPage<HudPageView>();

        var hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.UpdateFood(_foodAmount);
            hud.ShowHint();
        }

        SaveGame();

        AnalyticsManager.RunStarted(false, _currentLevel);
    }

    public void ContinueGame()
    {
        if (!SaveManager.HasSave())
        {
            return;
        }

        var data = SaveManager.LoadGame();

        if (data == null)
        {
            return;
        }

        _isGameOver = false;
        _currentLevel = data.Level;
        _foodAmount = data.Food;
        _revivesUsed = data.RevivesUsed;
        _boardManager.Seed = data.Seed;

        _boardManager.Clean();
        _boardManager.Init();

        _playerController.gameObject.SetActive(true);
        _playerController.Init();
        _playerController.Spawn(_boardManager, new Vector2Int(data.PlayerX, data.PlayerY));

        ViewManager.Instance.OpenPage<HudPageView>();

        var hud = ViewManager.Instance.CurrentPage as HudPageView;

        if (hud != null)
        {
            hud.UpdateFood(_foodAmount);
        }

        AnalyticsManager.RunStarted(true, _currentLevel);
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

        var data = new SaveData
        {
            Seed = _boardManager.Seed,
            Food = _foodAmount,
            Level = _currentLevel,
            PlayerX = _playerController.Cell.x,
            PlayerY = _playerController.Cell.y,
            BestLevel = SaveManager.GetBestLevel(),
            RevivesUsed = _revivesUsed,
        };

        SaveManager.SaveGame(data);
    }
}