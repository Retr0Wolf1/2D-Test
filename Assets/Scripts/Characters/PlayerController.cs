// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

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

    private BoardManager _board;
    private Vector2Int _cellPosition;
    private bool _isGameOver;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isMoving;
    private Vector3 _moveTarget;

    public Vector2Int Cell => _cellPosition;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init()
    {
        _isGameOver = false;
        _isMoving = false;

        if (_animator != null)
        {
            _animator.SetBool(MovingHash, false);
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
        _isMoving = false;

        if (_animator != null)
        {
            _animator.SetBool(MovingHash, false);
        }
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        _board = boardManager;
        _cellPosition = cell;
        _isMoving = false;

        transform.position = _board.CellToWorld(_cellPosition);

        if (_animator != null)
        {
            _animator.SetBool(MovingHash, false);
        }
    }

    public void MoveTo(Vector2Int cell)
    {
        _cellPosition = cell;
        _moveTarget = _board.CellToWorld(_cellPosition);
        _isMoving = true;

        PlayFootstep();

        if (_animator != null)
        {
            _animator.SetBool(MovingHash, true);
        }
    }

    public void PlayAttack()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(AttackHash);
        }

        if (AudioManager.Instance != null && _attackSound != null)
        {
            AudioManager.Instance.SFXSource.PlayOneShot(_attackSound, 2f);
        }
    }

    private void Update()
    {
        if (_board == null)
        {
            return;
        }

        if (_isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }

        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _moveTarget, _moveSpeed * Time.deltaTime);

            if (transform.position == _moveTarget)
            {
                _isMoving = false;

                if (_animator != null)
                {
                    _animator.SetBool(MovingHash, false);
                }

                var cellData = _board.GetCellData(_cellPosition);

                if (cellData != null && cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }

                GameManager.Instance.TurnManager.Tick();
            }

            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        var newCellTarget = _cellPosition;
        var hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
            FlipSprite(false);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
            FlipSprite(true);
        }

        if (!hasMoved)
        {
            return;
        }

        var targetCellData = _board.GetCellData(newCellTarget);

        if (targetCellData == null)
        {
            return;
        }

        if (targetCellData.ContainedObject != null)
        {
            if (targetCellData.ContainedObject.PlayerWantsToEnter())
            {
                MoveTo(newCellTarget);
            }
            else if (targetCellData.ContainedObject.IsAttackable)
            {
                PlayAttack();
                GameManager.Instance.TurnManager.Tick();
            }
        }
        else if (targetCellData.Passable)
        {
            MoveTo(newCellTarget);
        }
    }

    private void PlayFootstep()
    {
        if (AudioManager.Instance == null || _footstepSounds == null || _footstepSounds.Length == 0)
        {
            return;
        }

        var index = Random.Range(0, _footstepSounds.Length);
        AudioManager.Instance.SFXSource.PlayOneShot(_footstepSounds[index], 2f);
    }

    private void FlipSprite(bool flip)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = flip;
        }
    }
}