using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCellObject : CellObject
{
    public Tile EndTile;

    private bool m_IsOpen = false;

    public override bool IsAttackable => false;

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, EndTile);
        m_IsOpen = false;
    }

    public override bool PlayerWantsToEnter()
    {
        if (!m_IsOpen)
        {
            Debug.Log("Сначала убей всех врагов!");
            return false;
        }

        return true;
    }

    public override void PlayerEntered()
    {
        if (m_IsOpen)
            GameManager.Instance.NewLevel();
    }

    public void Open()
    {
        m_IsOpen = true;
        Debug.Log("Выход открыт!");
    }
}