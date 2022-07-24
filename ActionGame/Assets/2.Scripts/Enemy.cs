using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Enemy : MonoFSM<Enemy>
{
    #region  ����
    protected Action DeathAction { get; set; }
    [Header("ü��")]

    [SerializeField] protected int maxHp;
    public int MaxHp => maxHp;
    [SerializeField] protected int hp;
    public int Hp 
    {
        get => hp;
        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);
            if (hp <= 0)
                DeathAction?.Invoke();
        }
    }
    [Header("���ݷ�")]
    [SerializeField] protected int attackDamage;
    public int AttackDamage => attackDamage;
    [Header("����")]
    [SerializeField] protected int armor;
    public int Armor => armor;
    [Header("�̵��ӵ�")]
    [SerializeField] protected float moveSpeed = 1;
    public float MoveSpeed => moveSpeed;
    [Header("�÷��̾� ��������")]
    [SerializeField] protected float detectionDist = 3; //�÷��̾� ���� ����
    [Header("�ǰ� ������")]
    [SerializeField] float hitDelay = 0.1f;
    public float HitDelay => hitDelay;
    public float DetectionDist => detectionDist;
    [HideInInspector] public Player target;
    [HideInInspector] public bool IsDeath;
    #region Component
    protected SpriteRenderer spriteRenderer;
    public SpriteRenderer m_SpriteRenderer => spriteRenderer;
    protected Animator anim;
    public Animator Anim => anim;
    #endregion
    #endregion
    public override void SetState(IState<Enemy> state)
    {
        if (IsDeath)
            return;
        base.SetState(state);
    }
    public abstract void Hit(int damage);
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    protected virtual void Start()
    {
        SetState(new EnemyIdleState());
    }
}

#region FSM
public class EnemyIdleState : IState<Enemy>
{
    public Enemy Instance { get; set; }

    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
    }

    public virtual void OnExit()
    {

    }

    public virtual void OnUpdate()
    {
        //�ֺ��� �÷��̾ �ִ��� ����
        RaycastHit2D hit = Physics2D.CircleCast(Instance.transform.position, 5, Vector2.zero, 5, LayerMask.GetMask("Player"));
        if (hit)
        { //�÷��̾ �����Ǿ����� �÷��̾ Ÿ������ �����ϰ� �̵����·� ����
            Instance.target = hit.collider.GetComponent<Player>();
            Instance.SetState(new EnemyMoveState());
        }
    }
}

public class EnemyMoveState : IState<Enemy>
{
    public Enemy Instance { get; set; }

    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        Instance.Anim.SetBool("Run", true);
    }

    public virtual void OnExit()
    {
        Instance.Anim.SetBool("Run", false);
    }

    public virtual void OnUpdate()
    {
        //dist = �ڽŰ� Ÿ��(�÷��̾�)���� �Ÿ�
        float dist = Vector2.Distance(Instance.transform.position, Instance.target.transform.position);
        if (!Instance.target || dist > 5) //Ÿ���� null�̰ų� �Ÿ��� 5�̻��̸�
            Instance.SetState(new EnemyIdleState()); //Idle�� ���� ( �̵� �ߴ� ) 
        if (dist < 0.5f) //�Ÿ��� 0.5f �̸��̸�
            Instance.SetState(new EnemyAttackState()); //���ݻ��·� ����

        //Ÿ���� ���� �̵�
        Vector3 dir = (Instance.target.transform.position - Instance.transform.position).normalized;
        Instance.transform.position += dir * Instance.MoveSpeed * Time.deltaTime;
        if (dir.x != 0)
            Instance.m_SpriteRenderer.flipX = dir.x < 0;
    }
}

public class EnemyAttackState : IState<Enemy>
{
    public Enemy Instance { get; set; }
    Coroutine attackCor;

    protected IEnumerator C_Attack()
    {
        /* Instance.target?.SetState();
         *  [ ��� ��� ���� ���� ]
         * if(Instance.target != null) {
         *   Instance.target.SetState();
         * }
         */
        Instance.target?.SetState(new PlayerHitState()); //target�� null�� �ƴϸ� target�� ���¸� �ǰݻ��·� ����
        yield return new WaitForSeconds(1); //1�� ���
        Instance.SetState(new EnemyIdleState()); //Idle���·� ����
    }
    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        attackCor = Instance.StartCoroutine(C_Attack()); //�ڷ�ƾ ����
    }

    public virtual void OnExit()
    {
        //�߰��� ���°� ���ϸ� �����ڷ�ƾ ����
        Instance.StopCoroutine(attackCor);
    }

    public virtual void OnUpdate()
    {
    }
}
#endregion