// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;

public class WallObject : CellObject
{
    [FormerlySerializedAs("ObstacleTile")]
    [SerializeField] private Tile _obstacleTile;

    [FormerlySerializedAs("MaxHealth")]
    [SerializeField] private int _maxHealth = 3;

    [FormerlySerializedAs("NextWallPrefab")]
    [SerializeField] private WallObject _nextWallPrefab;

    private int _healthPoint;
    private Tile _originalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        _healthPoint = _maxHealth;
        _originalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, _obstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        _healthPoint -= 1;

        if (_healthPoint > 0)
        {
            ReplaceWithNext();
            return false;
        }

        BreakWall();
        return true;
    }

    private void ReplaceWithNext()
    {
        if (_nextWallPrefab == null)
        {
            return;
        }

        BoardManager board = GameManager.Instance.BoardManager;

        WallObject newWall = Instantiate(_nextWallPrefab);

        board.SetCellTile(_cell, _originalTile);

        BoardManager.CellData cellData = board.GetCellData(_cell);
        cellData.ContainedObject = newWall;

        newWall.transform.position = transform.position;
        newWall.Init(_cell);

        Destroy(gameObject);
    }

    private void BreakWall()
    {
        BoardManager board = GameManager.Instance.BoardManager;
        board.SetCellTile(_cell, _originalTile);

        BoardManager.CellData cellData = board.GetCellData(_cell);

        if (cellData != null)
        {
            cellData.Passable = true;
            cellData.ContainedObject = null;
        }

        Destroy(gameObject);
    }
}