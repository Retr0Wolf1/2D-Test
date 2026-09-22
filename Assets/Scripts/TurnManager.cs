// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;

public class TurnManager
{
    private int _turnCount;

    public event System.Action OnTick;

    public TurnManager()
    {
        _turnCount = 1;
    }

    public void Tick()
    {
        _turnCount += 1;
        Debug.Log("Current turn count : " + _turnCount);
        OnTick?.Invoke();
    }
}