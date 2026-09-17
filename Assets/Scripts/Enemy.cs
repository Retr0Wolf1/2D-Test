using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    public float MoveSpeed = 3f;
    public float MoveInterval = 1f;
    public float RandomChance = 0.3f;
    public int AttackDamage = 3;

    public bool SpriteFacesRight = true;
    public EnemyHealthBar HealthBarPrefab;
    public AudioClip[] FootstepSounds;
    public AudioClip AttackSound;

    public static int AliveCount = 0;

    public static void ResetAliveCount()
    {
        AliveCount = 0;
    }

    private int m_CurrentHealth;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    private float m_MoveTimer;

    private Animator m_Animator;
    private Vector3 m_OriginalScale;
    private EnemyHealthBar m_HealthBar;
    private AudioSource m_AudioSource;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
        m_OriginalScale = transform.localScale;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
        m_IsMoving = false;
        m_MoveTimer = 0f;

        AliveCount++;

        if (HealthBarPrefab != null)
        {
            m_HealthBar = Instantiate(HealthBarPrefab);
            m_HealthBar.Setup(transform, Camera.main);
            m_HealthBar.SetHealth(m_CurrentHealth, Health);
        }
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_HealthBar != null)
            m_HealthBar.SetHealth(m_CurrentHealth, Health);

        if (m_CurrentHealth <= 0)
        {
            if (m_HealthBar != null)
                Destroy(m_HealthBar.gameObject);

            AliveCount--;

            if (AliveCount <= 0)
                GameManager.Instance.BoardManager.OnAllEnemiesDead();

            Destroy(gameObject);
        }

        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        if (coord.x > m_Cell.x)
            FaceDirection(true);
        else if (coord.x < m_Cell.x)
            FaceDirection(false);

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;
        m_MoveTarget = board.CellToWorld(coord);
        m_IsMoving = true;

        PlayFootstep();

        return true;
    }

    void PlayFootstep()
    {
        if (m_AudioSource == null || FootstepSounds == null || FootstepSounds.Length == 0)
            return;

        int index = Random.Range(0, FootstepSounds.Length);
        m_AudioSource.PlayOneShot(FootstepSounds[index], 1.5f);
    }

    void FaceDirection(bool faceRight)
    {
        float absX = Mathf.Abs(m_OriginalScale.x);

        if (SpriteFacesRight)
        {
            if (faceRight)
                transform.localScale = new Vector3(-absX, m_OriginalScale.y, m_OriginalScale.z);
            else
                transform.localScale = new Vector3(absX, m_OriginalScale.y, m_OriginalScale.z);
        }
        else
        {
            if (faceRight)
                transform.localScale = new Vector3(absX, m_OriginalScale.y, m_OriginalScale.z);
            else
                transform.localScale = new Vector3(-absX, m_OriginalScale.y, m_OriginalScale.z);
        }
    }

    void Update()
    {
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                m_MoveTarget,
                MoveSpeed * Time.deltaTime
            );

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
            }
            return;
        }

        m_MoveTimer += Time.deltaTime;
        if (m_MoveTimer >= MoveInterval)
        {
            m_MoveTimer = 0f;
            TurnHappened();
        }
    }

    void TurnHappened()
    {
        if (Random.value < RandomChance)
        {
            RandomMove();
            return;
        }

        ChasePlayer();
    }

    void RandomMove()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        int startIndex = Random.Range(0, directions.Length);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[(startIndex + i) % directions.Length];

            if (MoveTo(m_Cell + dir))
            {
                return;
            }
        }
    }

    void ChasePlayer()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        bool isAdjacent =
            (xDist == 0 && absYDist == 1) ||
            (yDist == 0 && absXDist == 1);

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

    void AttackPlayer()
    {
        if (m_Animator != null)
            m_Animator.SetTrigger(AttackHash);

        if (m_AudioSource != null && AttackSound != null)
            m_AudioSource.PlayOneShot(AttackSound, 2f);

        GameManager.Instance.ChangeFood(-AttackDamage);
    }

    bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
            return MoveTo(m_Cell + Vector2Int.right);
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
            return MoveTo(m_Cell + Vector2Int.up);
        return MoveTo(m_Cell + Vector2Int.down);
    }
}