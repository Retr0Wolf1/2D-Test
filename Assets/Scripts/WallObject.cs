using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile ObstacleTile;
    public int MaxHealth = 3;

    public WallObject NextWallPrefab;

    private int m_HealthPoint;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        m_HealthPoint = MaxHealth;
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint -= 1;

        if (m_HealthPoint > 0)
        {
            ReplaceWithNext();
            return false;
        }

        BreakWall();
        return true;
    }

    void ReplaceWithNext()
    {
        if (NextWallPrefab == null)
            return;

        var board = GameManager.Instance.BoardManager;

        WallObject newWall = Instantiate(NextWallPrefab);

        board.SetCellTile(m_Cell, m_OriginalTile);

        var cellData = board.GetCellData(m_Cell);
        cellData.ContainedObject = newWall;

        newWall.transform.position = transform.position;
        newWall.Init(m_Cell);

        Destroy(gameObject);
    }

    void BreakWall()
    {
        var board = GameManager.Instance.BoardManager;
        board.SetCellTile(m_Cell, m_OriginalTile);

        var cellData = board.GetCellData(m_Cell);
        if (cellData != null)
        {
            cellData.Passable = true;
            cellData.ContainedObject = null;
        }

        Destroy(gameObject);
    }
}