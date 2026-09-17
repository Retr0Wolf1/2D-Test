using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    public Tilemap FogTilemap;
    public TileBase FogTile;
    public int VisionRadius = 3;
    public int SoftEdge = 3;

    private BoardManager m_Board;
    private Vector2Int m_LastPlayerCell = new Vector2Int(-999, -999);

    public void Setup(BoardManager board)
    {
        m_Board = board;

        if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
        {
            m_LastPlayerCell = GameManager.Instance.PlayerController.Cell;
            RefreshFog(m_LastPlayerCell);
        }
    }

    private void LateUpdate()
    {
        if (m_Board == null) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.PlayerController == null) return;
        if (FogTilemap == null || FogTile == null) return;

        Vector2Int playerCell = GameManager.Instance.PlayerController.Cell;

        if (playerCell != m_LastPlayerCell)
        {
            m_LastPlayerCell = playerCell;
            RefreshFog(playerCell);
        }
    }

    void RefreshFog(Vector2Int playerCell)
    {
        if (FogTilemap == null || FogTile == null || m_Board == null)
            return;

        float fullRadius = VisionRadius;
        float fadeEnd = VisionRadius + SoftEdge;

        for (int y = 0; y < m_Board.Height; y++)
        {
            for (int x = 0; x < m_Board.Width; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                float dx = x - playerCell.x;
                float dy = y - playerCell.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= fullRadius)
                {
                    FogTilemap.SetTile(tilePos, null);
                }
                else
                {
                    float t = Mathf.Clamp01((dist - fullRadius) / SoftEdge);
                    float alpha = Mathf.Lerp(0f, 1f, t);

                    FogTilemap.SetTile(tilePos, FogTile);
                    FogTilemap.SetColor(tilePos, new Color(0, 0, 0, alpha));
                }
            }
        }
    }
}