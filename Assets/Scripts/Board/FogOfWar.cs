// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class FogOfWar : MonoBehaviour
{
    [FormerlySerializedAs("FogTilemap")]
    [SerializeField] private Tilemap _fogTilemap;

    [FormerlySerializedAs("FogTile")]
    [SerializeField] private TileBase _fogTile;

    [FormerlySerializedAs("VisionRadius")]
    [SerializeField] private int _visionRadius = 3;

    [FormerlySerializedAs("SoftEdge")]
    [SerializeField] private int _softEdge = 3;

    private BoardManager _board;
    private Vector2Int _lastPlayerCell = new Vector2Int(-999, -999);

    public void Setup(BoardManager board)
    {
        _board = board;

        if (GameManager.Instance != null && GameManager.Instance.PlayerController != null)
        {
            _lastPlayerCell = GameManager.Instance.PlayerController.Cell;
            RefreshFog(_lastPlayerCell);
        }
    }

    private void LateUpdate()
    {
        if (_board == null)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.PlayerController == null)
        {
            return;
        }

        if (_fogTilemap == null || _fogTile == null)
        {
            return;
        }

        var playerCell = GameManager.Instance.PlayerController.Cell;

        if (playerCell != _lastPlayerCell)
        {
            _lastPlayerCell = playerCell;
            RefreshFog(playerCell);
        }
    }

    private void RefreshFog(Vector2Int playerCell)
    {
        if (_fogTilemap == null || _fogTile == null || _board == null)
        {
            return;
        }

        var fullRadius = (float)_visionRadius;

        for (var y = 0; y < _board.Height; y++)
        {
            for (var x = 0; x < _board.Width; x++)
            {
                var tilePos = new Vector3Int(x, y, 0);

                var dx = x - playerCell.x;
                var dy = y - playerCell.y;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= fullRadius)
                {
                    _fogTilemap.SetTile(tilePos, null);
                }
                else
                {
                    var t = Mathf.Clamp01((dist - fullRadius) / _softEdge);
                    var alpha = Mathf.Lerp(0f, 1f, t);

                    _fogTilemap.SetTile(tilePos, _fogTile);
                    _fogTilemap.SetColor(tilePos, new Color(0, 0, 0, alpha));
                }
            }
        }
    }
}