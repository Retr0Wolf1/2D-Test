// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [FormerlySerializedAs("Width")]
    [SerializeField] private int _width;

    [FormerlySerializedAs("Height")]
    [SerializeField] private int _height;

    [FormerlySerializedAs("GroundTiles")]
    [SerializeField] private Tile[] _groundTiles;

    [FormerlySerializedAs("WallTiles")]
    [SerializeField] private Tile[] _wallTiles;

    [FormerlySerializedAs("FoodPrefabs")]
    [SerializeField] private FoodObject[] _foodPrefabs;

    [FormerlySerializedAs("WallPrefabs")]
    [SerializeField] private WallObject[] _wallPrefabs;

    [FormerlySerializedAs("ExitCellPrefab")]
    [SerializeField] private ExitCellObject _exitCellPrefab;

    [FormerlySerializedAs("EnemyPrefab")]
    [SerializeField] private Enemy _enemyPrefab;

    [FormerlySerializedAs("MinFood")]
    [SerializeField] private int _minFood = 3;

    [FormerlySerializedAs("MaxFood")]
    [SerializeField] private int _maxFood = 8;

    [FormerlySerializedAs("MinEnemies")]
    [SerializeField] private int _minEnemies = 2;

    [FormerlySerializedAs("MaxEnemies")]
    [SerializeField] private int _maxEnemies = 4;

    [FormerlySerializedAs("Seed")]
    [SerializeField] private int _seed;

    private CellData[,] _boardData;
    private Tilemap _tilemap;
    private Grid _grid;
    private List<Vector2Int> _emptyCellsList;
    private System.Random _rng;
    private ExitCellObject _exitCell;

    public int Width => _width;
    public int Height => _height;

    public int Seed
    {
        get => _seed;
        set => _seed = value;
    }

    public void Init()
    {
        if (_seed == 0)
        {
            _seed = UnityEngine.Random.Range(1, 999999);
        }

        _rng = new System.Random(_seed);

        Enemy.ResetAliveCount();

        _tilemap = GetComponentInChildren<Tilemap>();
        _grid = GetComponentInChildren<Grid>();
        _emptyCellsList = new List<Vector2Int>();

        _boardData = new CellData[_width, _height];

        for (int y = 0; y < _height; ++y)
        {
            for (int x = 0; x < _width; ++x)
            {
                Tile tile;
                _boardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == _width - 1 || y == _height - 1)
                {
                    tile = _wallTiles[_rng.Next(0, _wallTiles.Length)];
                    _boardData[x, y].Passable = false;
                }
                else
                {
                    tile = _groundTiles[_rng.Next(0, _groundTiles.Length)];
                    _boardData[x, y].Passable = true;
                    _emptyCellsList.Add(new Vector2Int(x, y));
                }

                _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        _emptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(_width - 2, _height - 2);
        _exitCell = Instantiate(_exitCellPrefab);
        AddObject(_exitCell, endCoord);
        _emptyCellsList.Remove(endCoord);

        GenerateWall();
        GenerateFood();
        GenerateEnemies();

        SetupCamera();
        SetupFog();
    }

    public void Clean()
    {
        if (_boardData == null)
        {
            return;
        }

        for (int y = 0; y < _height; ++y)
        {
            for (int x = 0; x < _width; ++x)
            {
                CellData cellData = _boardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    public void OnAllEnemiesDead()
    {
        if (_exitCell != null)
        {
            _exitCell.Open();
        }

        UIManager.Instance.ShowGoToExit();
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return _grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= _width ||
            cellIndex.y < 0 || cellIndex.y >= _height)
        {
            return null;
        }

        return _boardData[cellIndex.x, cellIndex.y];
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        _tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return _tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    private void SetupCamera()
    {
        if (Camera.main == null)
        {
            return;
        }

        CameraFollow follow = Camera.main.GetComponent<CameraFollow>();

        if (follow == null)
        {
            return;
        }

        follow.Setup(this);

        if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
        {
            follow.SetTarget(GameManager.Instance.PlayerController.transform);
        }
    }

    private void SetupFog()
    {
        FogOfWar fog = FindAnyObjectByType<FogOfWar>();

        if (fog != null)
        {
            fog.Setup(this);
        }
    }

    private void GenerateWall()
    {
        int totalCells = _width * _height;
        int minWalls = Mathf.RoundToInt(totalCells * 0.05f);
        int maxWalls = Mathf.RoundToInt(totalCells * 0.10f);

        int wallCount = _rng.Next(minWalls, maxWalls + 1);

        for (int i = 0; i < wallCount; ++i)
        {
            if (_emptyCellsList.Count == 0)
            {
                break;
            }

            int randomIndex = _rng.Next(0, _emptyCellsList.Count);
            Vector2Int coord = _emptyCellsList[randomIndex];

            _emptyCellsList.RemoveAt(randomIndex);

            int wallIndex = _rng.Next(0, _wallPrefabs.Length);
            WallObject newWall = Instantiate(_wallPrefabs[wallIndex]);
            AddObject(newWall, coord);
        }
    }

    private void GenerateFood()
    {
        int foodCount = _rng.Next(_minFood, _maxFood + 1);

        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = _rng.Next(0, _emptyCellsList.Count);
            Vector2Int coord = _emptyCellsList[randomIndex];

            _emptyCellsList.RemoveAt(randomIndex);
            int prefabIndex = _rng.Next(0, _foodPrefabs.Length);
            FoodObject newFood = Instantiate(_foodPrefabs[prefabIndex]);
            AddObject(newFood, coord);
        }
    }

    private void GenerateEnemies()
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("EnemyPrefab не назначен!");
            return;
        }

        int enemyCount = _rng.Next(_minEnemies, _maxEnemies + 1);
        enemyCount = Mathf.Min(enemyCount, _emptyCellsList.Count);

        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = _rng.Next(0, _emptyCellsList.Count);
            Vector2Int coord = _emptyCellsList[randomIndex];

            _emptyCellsList.RemoveAt(randomIndex);

            Enemy newEnemy = Instantiate(_enemyPrefab);
            AddObject(newEnemy, coord);
        }
    }

    private void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = _boardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }
}