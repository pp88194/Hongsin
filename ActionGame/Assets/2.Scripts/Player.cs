using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoFSM<Player>
{
    #region ����
    [Header("ü��")]
    [SerializeField] int maxHp;
    public int MaxHp => maxHp;
    [SerializeField] int hp;
    public int Hp
    {
        get => hp;
        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);
        }
    }
    [Header("���ݷ�")]
    [SerializeField] int attackDamage;
    public int AttackDamage => attackDamage;
    [Header("���ݼӵ�(�������� ����)")]
    [SerializeField] float attackSpeed = 1;
    public float AttackSpeed
    {
        get => attackSpeed;
        set
        {
            attackSpeed = Mathf.Clamp(value, 1, 5);
            Anim.SetFloat("AttackSpeed", value);
        }
    }
    [Header("�̵��ӵ�")]
    [SerializeField] float moveSpeed;
    public float MoveSpeed => moveSpeed;
    [Header("�ǰݵ�����")]
    [SerializeField] float hitDelay = 0.1f;
    public float HitDelay => hitDelay;

    Vector2 moveDir;
    public Vector2 MoveDir => moveDir;
    Vector2 lastMoveDir;
    public Vector2 LastMoveDir => lastMoveDir;
    int xDir;
    public int XDir => xDir;

    #region Component
    SpriteRenderer spriteRenderer;
    public SpriteRenderer m_SpriteRenderer => spriteRenderer;
    Animator anim;
    public Animator Anim => anim;
    #endregion
    #endregion
    void InputDir()
    {
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (moveDir != Vector2.zero)
            lastMoveDir = moveDir;
        if (moveDir.x != 0)
            xDir = moveDir.x > 0 ? 1 : -1;
    }
    public void Hit(int damage)
    {
        SetState(new PlayerHitState());
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        AttackSpeed = attackSpeed;
    }
    private void Start()
    {
        SetState(new PlayerIdleState());
    }
    protected override void Update()
    {
        InputDir();
        base.Update();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(xDir * 0.3f, 0), new Vector3(2f, 2));
    }
}

#region FSM
public class PlayerIdleState : IState<Player>
{
    public Player Instance { get; set; }

    public void OnEnter(Player player)
    {
        Instance = player;
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        //�̵����·� ����
        if (Instance.MoveDir != Vector2.zero)
            Instance.SetState(new PlayerMoveState());
        //��������·� ����
        if (Input.GetKeyDown(KeyCode.LeftShift))
            Instance.SetState(new PlayerRollState());
        //���ݻ��·� ����
        if (Input.GetKeyDown(KeyCode.Q))
            Instance.SetState(new PlayerAttackState());
    }
}

public class PlayerMoveState : IState<Player>
{
    public Player Instance { get; set; }

    public void OnEnter(Player player)
    {
        Instance = player;
        Instance.Anim.SetBool("isMove", true);
    }

    public void OnExit()
    {
        Instance.Anim.SetBool("isMove", false);
    }

    public void OnUpdate()
    {
        //��������·� ����
        if (Input.GetKeyDown(KeyCode.LeftShift))
            Instance.SetState(new PlayerRollState());
        //���ݻ��·� ����
        if (Input.GetKeyDown(KeyCode.Q))
            Instance.SetState(new PlayerAttackState());
        //�̵������� 0�̸� Idle���·� ����
        if (Instance.MoveDir == Vector2.zero)
            Instance.SetState(new PlayerIdleState());

        //�̵�
        Instance.transform.position += (Vector3)Instance.MoveDir * Instance.MoveSpeed * Time.deltaTime;
        if (Instance.MoveDir.x != 0)
            Instance.m_SpriteRenderer.flipX = Instance.MoveDir.x < 0;
    }
}

public class PlayerRollState : IState<Player>
{
    public Player Instance { get; set; }
    Coroutine rollCor;

    IEnumerator C_Roll()
    {
        Instance.Anim.SetBool("isRoll", true);
        Vector2 begin = Instance.transform.position; //��������
        Vector2 target = begin + Instance.LastMoveDir * 2; //������ ( �������� + �̵����� * 2 )
        //������������ ���������� �̵�
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            Instance.transform.position = Vector2.Lerp(begin, target, t * 2);
            yield return null;
        }
        Instance.Anim.SetBool("isRoll", false);
        Instance.SetState(new PlayerIdleState());
    }
    public void OnEnter(Player player)
    {
        Instance = player;
        rollCor = Instance.StartCoroutine(C_Roll());
    }

    public void OnExit()
    {
        Instance.Anim.SetBool("isRoll", false);
        Instance.StopCoroutine(rollCor);
    }

    public void OnUpdate()
    {

    }
}
public class PlayerAttackState : IState<Player>
{
    public Player Instance { get; set; }
    Coroutine attackCor;

    IEnumerator C_Attack()
    {
        int xDir = Instance.XDir;
        Instance.Anim.SetBool("isAttack", true);
        yield return new WaitForSeconds(0.7f / Instance.AttackSpeed);
        Collider2D[] hits = Physics2D.OverlapBoxAll(Instance.transform.position + new Vector3(xDir, 0), new Vector2(1.5f, 2), 0, LayerMask.GetMask("Enemy"));
        foreach(var hit in hits) 
        {
            hit.GetComponent<Enemy>().Hit(Instance.AttackDamage);
        }
        Instance.Anim.SetBool("isAttack", false);
        Instance.SetState(new PlayerIdleState());
    }
    public void OnEnter(Player player)
    {
        Instance = player;
        attackCor = Instance.StartCoroutine(C_Attack());
    }

    public void OnExit()
    {
        Instance.Anim.SetBool("isAttack", false);
        Instance.StopCoroutine(attackCor);
    }

    public void OnUpdate()
    {

    }
}

public class PlayerHitState : IState<Player>
{
    public Player Instance { get; set; }

    IEnumerator C_Hit()
    {
        Instance.Anim.SetTrigger("Hit");
        yield return new WaitForSeconds(Instance.HitDelay);
        Instance.SetState(new PlayerIdleState());
    }
    public void OnEnter(Player instance)
    {
        Instance = instance;
        Instance.StartCoroutine(C_Hit());
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
    }
}
#endregion