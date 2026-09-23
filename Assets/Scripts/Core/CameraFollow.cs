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
        var cam = GetComponent<Camera>();
        var halfHeight = cam.orthographicSize;
        var halfWidth = halfHeight * cam.aspect;

        var bottomLeft = board.CellToWorld(new Vector2Int(0, 0));
        var topRight = board.CellToWorld(new Vector2Int(board.Width - 1, board.Height - 1));

        var halfCell = 0.5f;

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

        var targetX = _target.position.x;
        var targetY = _target.position.y;

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

        var desired = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
    }
}