// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour
{
    [FormerlySerializedAs("Target")]
    [SerializeField] private Transform _target;

    [FormerlySerializedAs("SmoothSpeed")]
    [SerializeField] private float _smoothSpeed = 5f;

    private float _minX;
    private float _maxX;
    private float _minY;
    private float _maxY;
    private bool _isSetup;

    public void Setup(BoardManager board)
    {
        Camera cam = GetComponent<Camera>();
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 bottomLeft = board.CellToWorld(new Vector2Int(0, 0));
        Vector3 topRight = board.CellToWorld(new Vector2Int(board.Width - 1, board.Height - 1));

        float halfCell = 0.5f;

        _minX = bottomLeft.x - halfCell + halfWidth;
        _maxX = topRight.x + halfCell - halfWidth;
        _minY = bottomLeft.y - halfCell + halfHeight;
        _maxY = topRight.y + halfCell - halfHeight;

        _isSetup = true;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (!_isSetup || _target == null)
        {
            return;
        }

        float targetX = _target.position.x;
        float targetY = _target.position.y;

        if (_minX > _maxX)
        {
            targetX = (_minX + _maxX) / 2f;
        }
        else
        {
            targetX = Mathf.Clamp(targetX, _minX, _maxX);
        }

        if (_minY > _maxY)
        {
            targetY = (_minY + _maxY) / 2f;
        }
        else
        {
            targetY = Mathf.Clamp(targetY, _minY, _maxY);
        }

        Vector3 desired = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
    }
}