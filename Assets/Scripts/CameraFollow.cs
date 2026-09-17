using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public float SmoothSpeed = 5f;

    private float m_MinX;
    private float m_MaxX;
    private float m_MinY;
    private float m_MaxY;
    private bool m_IsSetup;

    public void Setup(BoardManager board)
    {
        Camera cam = GetComponent<Camera>();
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 bottomLeft = board.CellToWorld(new Vector2Int(0, 0));
        Vector3 topRight = board.CellToWorld(new Vector2Int(board.Width - 1, board.Height - 1));

        float halfCell = 0.5f;

        m_MinX = bottomLeft.x - halfCell + halfWidth;
        m_MaxX = topRight.x + halfCell - halfWidth;
        m_MinY = bottomLeft.y - halfCell + halfHeight;
        m_MaxY = topRight.y + halfCell - halfHeight;

        m_IsSetup = true;
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    private void LateUpdate()
    {
        if (!m_IsSetup || Target == null)
            return;

        float targetX = Target.position.x;
        float targetY = Target.position.y;

        if (m_MinX > m_MaxX)
            targetX = (m_MinX + m_MaxX) / 2f;
        else
            targetX = Mathf.Clamp(targetX, m_MinX, m_MaxX);

        if (m_MinY > m_MaxY)
            targetY = (m_MinY + m_MaxY) / 2f;
        else
            targetY = Mathf.Clamp(targetY, m_MinY, m_MaxY);

        Vector3 desired = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);
    }
}