// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [FormerlySerializedAs("MoveSpeed")]
    [SerializeField] private float _moveSpeed = 1f;

    [FormerlySerializedAs("FootstepSounds")]
    [SerializeField] private AudioClip[] _footstepSounds;

    [FormerlySerializedAs("AttackSound")]
    [SerializeField] private AudioClip _attackSound;

    [Header("Mobile")]
    [SerializeField] private Joystick _joystick;

    [Range(0.1f, 0.9f)]
    [SerializeField] private float _joystickDeadzone = 0.5f;

    [SerializeField] private float _moveCooldown = 0.15f;

    private BoardManager _board;
    private Vector2Int _cellPosition;
    private Vector2Int _lastDirection = Vector2Int.right;
    private bool _isGameOver;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isMoving;
    private Vector3 _moveTarget;
    private bool _attackQueued;
    private float _lastMoveTime;

    public Vector2Int Cell => _cellPosition;
    public Vector2Int LastDirection => _lastDirection;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init()
    {
        _isGameOver = false;
        _isMoving = false;
        _attackQueued = false;
        _lastMoveTime = 0f;

        if (_animator != null) _animator.SetBool(MovingHash, false);
    }

    public void GameOver()
    {
        _isGameOver = true;
        _isMoving = false;
        _attackQueued = false;

        if (_animator != null) _animator.SetBool(MovingHash, false);
    }

    public void SetJoystick(Joystick joystick)
    {
        _joystick = joystick;
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        _board = boardManager;
        _cellPosition = cell;
        _isMoving = false;
        _attackQueued = false;
        _lastMoveTime = 0f;

        transform.DOKill();
        transform.position = _board.CellToWorld(_cellPosition);

        if (_animator != null) _animator.SetBool(MovingHash, false);
    }

    public void MoveTo(Vector2Int cell)
    {
        _cellPosition = cell;
        _moveTarget = _board.CellToWorld(_cellPosition);
        _isMoving = true;

        PlayFootstep();

        if (_animator != null) _animator.SetBool(MovingHash, true);

        var duration = 1f / _moveSpeed;

        transform.DOMove(_moveTarget, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(OnMoveComplete);
    }

    public void PlayAttack()
    {
        if (_animator != null) _animator.SetTrigger(AttackHash);

        if (AudioManager.Instance != null && _attackSound != null)
        {
            AudioManager.Instance.SFXSource.PlayOneShot(_attackSound, 2f);
        }
    }

    public void OnAttackButton()
    {
        if (_board == null || _isGameOver) return;

        if (_isMoving)
        {
            _attackQueued = true;
            return;
        }

        TryAttackAt(_cellPosition + _lastDirection);
    }

    private void OnMoveComplete()
    {
        _isMoving = false;

        if (_animator != null) _animator.SetBool(MovingHash, false);

        var cellData = _board.GetCellData(_cellPosition);

        if (cellData != null && cellData.ContainedObject != null)
        {
            cellData.ContainedObject.PlayerEntered();
        }

        GameManager.Instance.TurnManager.Tick();

        if (_attackQueued)
        {
            _attackQueued = false;
            TryAttackAt(_cellPosition + _lastDirection);
        }
    }

    private void Update()
    {
        if (_board == null) return;

        if (_isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                OnAttackButton();
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ViewManager.Instance.OpenPopup<PausePopupView>();
            }
        }

        if (_isMoving) return;

        if (Time.time - _lastMoveTime < _moveCooldown) return;

        Vector2Int direction = ReadInput();

        if (direction == Vector2Int.zero) return;

        _lastMoveTime = Time.time;

        _lastDirection = direction;

        if (direction.x > 0) FlipSprite(false);
        else if (direction.x < 0) FlipSprite(true);

        TryMoveOrAttack(direction);
    }

    private void TryMoveOrAttack(Vector2Int direction)
    {
        var newCellTarget = _cellPosition + direction;
        var targetCellData = _board.GetCellData(newCellTarget);

        if (targetCellData == null) return;

        if (targetCellData.ContainedObject == null)
        {
            if (targetCellData.Passable) MoveTo(newCellTarget);
            return;
        }

        if (targetCellData.ContainedObject is FoodObject ||
            targetCellData.ContainedObject is ExitCellObject)
        {
            if (targetCellData.ContainedObject.PlayerWantsToEnter())
            {
                MoveTo(newCellTarget);
            }
        }
    }

    private bool TryAttackAt(Vector2Int cell)
    {
        var cellData = _board.GetCellData(cell);

        if (cellData == null || cellData.ContainedObject == null) return false;
        if (!cellData.ContainedObject.IsAttackable) return false;

        PlayAttack();
        cellData.ContainedObject.PlayerWantsToEnter();
        GameManager.Instance.TurnManager.Tick();

        return true;
    }

    private Vector2Int ReadInput()
    {
        if (_joystick != null)
        {
            float h = _joystick.Horizontal;
            float v = _joystick.Vertical;

            if (Mathf.Abs(h) > _joystickDeadzone || Mathf.Abs(v) > _joystickDeadzone)
            {
                if (Mathf.Abs(h) > Mathf.Abs(v))
                {
                    return h > 0 ? Vector2Int.right : Vector2Int.left;
                }

                return v > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) return Vector2Int.up;
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) return Vector2Int.down;
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) return Vector2Int.right;
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) return Vector2Int.left;
        }

        return Vector2Int.zero;
    }

    private void PlayFootstep()
    {
        if (AudioManager.Instance == null || _footstepSounds == null || _footstepSounds.Length == 0) return;

        var index = Random.Range(0, _footstepSounds.Length);
        AudioManager.Instance.SFXSource.PlayOneShot(_footstepSounds[index], 2f);
    }

    private void FlipSprite(bool flip)
    {
        if (_spriteRenderer != null) _spriteRenderer.flipX = flip;
    }
}