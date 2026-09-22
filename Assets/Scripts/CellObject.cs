// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;

public class CellObject : MonoBehaviour
{
    protected Vector2Int _cell;

    public virtual bool IsAttackable => true;

    public virtual void Init(Vector2Int coord)
    {
        _cell = coord;
    }

    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }

    public virtual void PlayerEntered()
    {
    }
}