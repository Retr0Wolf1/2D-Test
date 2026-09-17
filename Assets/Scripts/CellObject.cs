using UnityEngine;

public class CellObject : MonoBehaviour
{
    protected Vector2Int m_Cell;

    public virtual bool IsAttackable => true;

    public virtual void Init(Vector2Int coord)
    {
        m_Cell = coord;
    }

    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }

    public virtual void PlayerEntered()
    {
    }
}