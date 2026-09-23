// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class ExitCellObject : CellObject
{
    [FormerlySerializedAs("EndTile")]
    [SerializeField] private Tile _endTile;

    private bool _isOpen;

    public override bool IsAttackable => false;

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, _endTile);
        _isOpen = false;
    }

    public override bool PlayerWantsToEnter()
    {
        if (!_isOpen)
        {
            return false;
        }

        return true;
    }

    public override void PlayerEntered()
    {
        if (_isOpen)
        {
            GameManager.Instance.NewLevel();
        }
    }

    public void Open()
    {
        _isOpen = true;
    }
}