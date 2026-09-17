using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    public FoodObject[] FoodPrefabs;
    public WallObject[] WallPrefabs;
    public ExitCellObject ExitCellPrefab;

    public Enemy EnemyPrefab;

    public int MinFood = 3;
    public int MaxFood = 8;

    public int MinEnemies = 2;
    public int MaxEnemies = 4;

    private ExitCellObject m_ExitCell;

    public void Init()
    {
        Enemy.ResetAliveCount();

        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();

        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        m_ExitCell = Instantiate(ExitCellPrefab);
        AddObject(m_ExitCell, endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall();
        GenerateFood();
        GenerateEnemies();

        SetupCamera();
        SetupFog();
    }

    void SetupCamera()
    {
        if (Camera.main == null)
            return;

        CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
        if (follow == null)
            return;

        follow.Setup(this);

        if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
            follow.SetTarget(GameManager.Instance.PlayerController.transform);
    }

    void SetupFog()
    {
        FogOfWar fog = FindAnyObjectByType<FogOfWar>();
        if (fog != null)
            fog.Setup(this);
    }

    public void OnAllEnemiesDead()
    {
        if (m_ExitCell != null)
            m_ExitCell.Open();
    }

    public void Clean()
    {
        if (m_BoardData == null)
            return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    void GenerateWall()
    {
        int totalCells = Width * Height;
        int minWalls = Mathf.RoundToInt(totalCells * 0.05f);
        int maxWalls = Mathf.RoundToInt(totalCells * 0.10f);

        int wallCount = Random.Range(minWalls, maxWalls + 1);

        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0)
                break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int wallIndex = Random.Range(0, WallPrefabs.Length);
            WallObject newWall = Instantiate(WallPrefabs[wallIndex]);
            AddObject(newWall, coord);
        }
    }

    void GenerateFood()
    {
        int foodCount = Random.Range(MinFood, MaxFood + 1);

        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            int prefabIndex = Random.Range(0, FoodPrefabs.Length);
            FoodObject newFood = Instantiate(FoodPrefabs[prefabIndex]);
            AddObject(newFood, coord);
        }
    }

    void GenerateEnemies()
    {
        if (EnemyPrefab == null)
        {
            Debug.LogWarning("EnemyPrefab не назначен в BoardManager!");
            return;
        }

        int enemyCount = Random.Range(MinEnemies, MaxEnemies + 1);
        enemyCount = Mathf.Min(enemyCount, m_EmptyCellsList.Count);

        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            Enemy newEnemy = Instantiate(EnemyPrefab);
            AddObject(newEnemy, coord);
        }
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }
}