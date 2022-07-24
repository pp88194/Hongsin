using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Enemy : MonoFSM<Enemy>
{
    #region  변수
    protected Action DeathAction { get; set; }
    [Header("체력")]

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
    [Header("공격력")]
    [SerializeField] protected int attackDamage;
    public int AttackDamage => attackDamage;
    [Header("방어력")]
    [SerializeField] protected int armor;
    public int Armor => armor;
    [Header("이동속도")]
    [SerializeField] protected float moveSpeed = 1;
    public float MoveSpeed => moveSpeed;
    [Header("플레이어 감지범위")]
    [SerializeField] protected float detectionDist = 3; //플레이어 감지 범위
    [Header("피격 딜레이")]
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
        //주변에 플레이어가 있는지 감지
        RaycastHit2D hit = Physics2D.CircleCast(Instance.transform.position, 5, Vector2.zero, 5, LayerMask.GetMask("Player"));
        if (hit)
        { //플레이어가 감지되었으면 플레이어를 타겟으로 지정하고 이동상태로 변경
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
        //dist = 자신과 타겟(플레이어)간의 거리
        float dist = Vector2.Distance(Instance.transform.position, Instance.target.transform.position);
        if (!Instance.target || dist > 5) //타겟이 null이거나 거리가 5이상이면
            Instance.SetState(new EnemyIdleState()); //Idle로 변경 ( 이동 중단 ) 
        if (dist < 0.5f) //거리가 0.5f 미만이면
            Instance.SetState(new EnemyAttackState()); //공격상태로 변경

        //타겟을 향해 이동
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
         *  [ ↑와 ↓는 서로 같음 ]
         * if(Instance.target != null) {
         *   Instance.target.SetState();
         * }
         */
        Instance.target?.SetState(new PlayerHitState()); //target이 null이 아니면 target의 상태를 피격상태로 만듦
        yield return new WaitForSeconds(1); //1초 대기
        Instance.SetState(new EnemyIdleState()); //Idle상태로 변경
    }
    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        attackCor = Instance.StartCoroutine(C_Attack()); //코루틴 시작
    }

    public virtual void OnExit()
    {
        //중간에 상태가 변하면 공격코루틴 종료
        Instance.StopCoroutine(attackCor);
    }

    public virtual void OnUpdate()
    {
    }
}
#endregion