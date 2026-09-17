using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;

    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;
    private AudioSource m_AudioSource;

    private bool m_IsMoving;
    private Vector3 m_MoveTarget;

    public float MoveSpeed = 1f;
    public AudioClip[] FootstepSounds;
    public AudioClip AttackSound;

    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public Vector2Int Cell => m_CellPosition;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void Init()
    {
        m_IsGameOver = false;
        m_IsMoving = false;

        if (m_Animator != null)
            m_Animator.SetBool(MovingHash, false);
    }

    public void GameOver()
    {
        m_IsGameOver = true;
        m_IsMoving = false;

        if (m_Animator != null)
            m_Animator.SetBool(MovingHash, false);
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;
        m_IsMoving = false;

        transform.position = m_Board.CellToWorld(m_CellPosition);

        if (m_Animator != null)
            m_Animator.SetBool(MovingHash, false);
    }

    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        m_IsMoving = true;

        PlayFootstep();

        if (m_Animator != null)
            m_Animator.SetBool(MovingHash, true);
    }

    public void PlayAttack()
    {
        if (m_Animator != null)
            m_Animator.SetTrigger(AttackHash);

        if (m_AudioSource != null && AttackSound != null)
            m_AudioSource.PlayOneShot(AttackSound, 2f);
    }

    void PlayFootstep()
    {
        if (m_AudioSource == null || FootstepSounds == null || FootstepSounds.Length == 0)
            return;

        int index = Random.Range(0, FootstepSounds.Length);
        m_AudioSource.PlayOneShot(FootstepSounds[index], 2f);
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current != null &&
                Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

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

                if (m_Animator != null)
                    m_Animator.SetBool(MovingHash, false);

                BoardManager.CellData cellData =
                    m_Board.GetCellData(m_CellPosition);

                if (cellData != null && cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }
            return;
        }

        if (Keyboard.current == null)
            return;

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

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

        if (hasMoved)
        {
            BoardManager.CellData cellData =
                m_Board.GetCellData(newCellTarget);

            if (cellData != null)
            {
                if (cellData.ContainedObject != null)
                {
                    if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        GameManager.Instance.TurnManager.Tick();
                        MoveTo(newCellTarget);
                    }
                    else if (cellData.ContainedObject.IsAttackable)
                    {
                        PlayAttack();
                        GameManager.Instance.TurnManager.Tick();
                    }
                }
                else if (cellData.Passable)
                {
                    GameManager.Instance.TurnManager.Tick();
                    MoveTo(newCellTarget);
                }
            }
        }
    }

    void FlipSprite(bool flip)
    {
        if (m_SpriteRenderer != null)
            m_SpriteRenderer.flipX = flip;
    }
}