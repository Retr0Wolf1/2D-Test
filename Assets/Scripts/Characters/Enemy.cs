// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : CellObject
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public static int AliveCount;

    [FormerlySerializedAs("Health")]
    [SerializeField] private int _health = 3;

    [FormerlySerializedAs("MoveSpeed")]
    [SerializeField] private float _moveSpeed = 3f;

    [FormerlySerializedAs("MoveInterval")]
    [SerializeField] private float _moveInterval = 1f;

    [FormerlySerializedAs("RandomChance")]
    [SerializeField] private float _randomChance = 0.3f;

    [FormerlySerializedAs("AttackDamage")]
    [SerializeField] private int _attackDamage = 3;

    [FormerlySerializedAs("SpriteFacesRight")]
    [SerializeField] private bool _spriteFacesRight = true;

    [FormerlySerializedAs("HealthBarPrefab")]
    [SerializeField] private EnemyHealthBar _healthBarPrefab;

    [FormerlySerializedAs("FootstepSounds")]
    [SerializeField] private AudioClip[] _footstepSounds;

    [FormerlySerializedAs("AttackSound")]
    [SerializeField] private AudioClip _attackSound;

    private int _currentHealth;
    private bool _isMoving;
    private Vector3 _moveTarget;
    private float _moveTimer;
    private Animator _animator;
    private Vector3 _originalScale;
    private EnemyHealthBar _healthBar;

    public static void ResetAliveCount()
    {
        AliveCount = 0;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _originalScale = transform.localScale;
    }

    private void Update()
    {
        if (_isMoving)
        {
            return;
        }

        _moveTimer += Time.deltaTime;

        if (_moveTimer >= _moveInterval)
        {
            _moveTimer = 0f;
            TurnHappened();
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);

        _currentHealth = _health;
        _isMoving = false;
        _moveTimer = 0f;

        AliveCount++;

        if (_healthBarPrefab != null)
        {
            _healthBar = Instantiate(_healthBarPrefab);
            _healthBar.Setup(transform, Camera.main);
            _healthBar.SetHealth(_currentHealth, _health);
        }
    }

    public override bool PlayerWantsToEnter()
    {
        _currentHealth -= 1;

        if (_healthBar != null)
        {
            _healthBar.SetHealth(_currentHealth, _health);
        }

        if (_currentHealth <= 0)
        {
            if (_healthBar != null)
            {
                Destroy(_healthBar.gameObject);
            }

            AliveCount--;

            if (AliveCount <= 0)
            {
                GameManager.Instance.BoardManager.OnAllEnemiesDead();
            }

            transform.DOComplete();
            Destroy(gameObject);
        }

        return false;
    }

    private bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        if (coord.x > _cell.x)
        {
            FaceDirection(true);
        }
        else if (coord.x < _cell.x)
        {
            FaceDirection(false);
        }

        var currentCell = board.GetCellData(_cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        _cell = coord;
        _moveTarget = board.CellToWorld(coord);
        _isMoving = true;

        PlayFootstep();

        var duration = 1f / _moveSpeed;

        transform.DOMove(_moveTarget, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(() => _isMoving = false);

        return true;
    }

    private void TurnHappened()
    {
        if (Random.value < _randomChance)
        {
            RandomMove();
            return;
        }

        ChasePlayer();
    }

    private void RandomMove()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        var startIndex = Random.Range(0, directions.Length);

        for (var i = 0; i < directions.Length; i++)
        {
            var dir = directions[(startIndex + i) % directions.Length];

            if (MoveTo(_cell + dir))
            {
                return;
            }
        }
    }

    private void ChasePlayer()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;

        var xDist = playerCell.x - _cell.x;
        var yDist = playerCell.y - _cell.y;

        var absXDist = Mathf.Abs(xDist);
        var absYDist = Mathf.Abs(yDist);

        var isAdjacent = (xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1);

        if (isAdjacent)
        {
            AttackPlayer();
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    private void AttackPlayer()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(AttackHash);
        }

        if (AudioManager.Instance != null && _attackSound != null)
        {
            AudioManager.Instance.SFXSource.PlayOneShot(_attackSound, 2f);
        }

        GameManager.Instance.ChangeFood(-_attackDamage);
    }

    private bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(_cell + Vector2Int.right);
        }

        return MoveTo(_cell + Vector2Int.left);
    }

    private bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(_cell + Vector2Int.up);
        }

        return MoveTo(_cell + Vector2Int.down);
    }

    private void PlayFootstep()
    {
        if (AudioManager.Instance == null || _footstepSounds == null || _footstepSounds.Length == 0)
        {
            return;
        }

        var index = Random.Range(0, _footstepSounds.Length);
        AudioManager.Instance.SFXSource.PlayOneShot(_footstepSounds[index], 1.5f);
    }

    private void FaceDirection(bool faceRight)
    {
        var absX = Mathf.Abs(_originalScale.x);

        if (_spriteFacesRight)
        {
            if (faceRight)
            {
                transform.localScale = new Vector3(-absX, _originalScale.y, _originalScale.z);
            }
            else
            {
                transform.localScale = new Vector3(absX, _originalScale.y, _originalScale.z);
            }
        }
        else
        {
            if (faceRight)
            {
                transform.localScale = new Vector3(absX, _originalScale.y, _originalScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-absX, _originalScale.y, _originalScale.z);
            }
        }
    }
}