using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigSlime : Enemy
{
    [SerializeField] GameObject miniSlimePrefab;
    public GameObject MiniSlimePrefab => miniSlimePrefab;

    public override void Hit(int damage)
    {
        Hp -= damage;
        SetState(new BigSlimeHitState());
    }
    protected override void Awake()
    {
        base.Awake();
        DeathAction += () => SetState(new BigSlimeDeathState());
    }
    protected override void Start()
    {
        SetState(new BigSlimeRangeAttack());
    }
}

#region FSM
public class BigSlimeIdleState : IState<Enemy>
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
            Instance.SetState(new BigSlimeMoveState());
        }
    }
}

public class BigSlimeMoveState : IState<Enemy>
{
    public Enemy Instance { get; set; }
    float rangeAttackDelay;

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
        rangeAttackDelay += Time.deltaTime;

        //dist = 자신과 타겟(플레이어)간의 거리
        float dist = Vector2.Distance(Instance.transform.position, Instance.target.transform.position);
        if (!Instance.target || dist > 5) //타겟이 null이거나 거리가 5이상이면
            Instance.SetState(new BigSlimeIdleState()); //Idle로 변경 ( 이동 중단 ) 
        if (rangeAttackDelay > 5f) //원거리공격대기시간이 다 차면 원거리공격
        {
            Instance.SetState(new BigSlimeRangeAttack());
            rangeAttackDelay = 0;
        }
        if (dist < 0.5f) //거리가 0.5f 미만이면
            Instance.SetState(new BigSlimeMeleeAttackState()); //공격상태로 변경

        //타겟을 향해 이동
        Vector3 dir = (Instance.target.transform.position - Instance.transform.position).normalized;
        Instance.transform.position += dir * Instance.MoveSpeed * Time.deltaTime;
        if (dir.x != 0)
            Instance.m_SpriteRenderer.flipX = dir.x < 0;
    }
}

public class BigSlimeMeleeAttackState : IState<Enemy>
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
        Instance.target?.Hit(Instance.AttackDamage); //target이 null이 아니면 target의 상태를 피격상태로 만듦
        yield return new WaitForSeconds(1); //1초 대기
        Instance.SetState(new BigSlimeIdleState()); //Idle상태로 변경
    }
    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        attackCor = Instance.StartCoroutine(C_Attack()); //코루틴 시작
        Instance.Anim.SetTrigger("Attack");
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
public class BigSlimeRangeAttack : IState<Enemy>
{
    public Enemy Instance { get; set; }
    Coroutine attackCor;
    IEnumerator C_Attack()
    {
        MiniSlime miniSlime = Object.Instantiate((Instance as BigSlime).MiniSlimePrefab, Instance.transform.position, Quaternion.identity).GetComponent<MiniSlime>();
        miniSlime.Init(Instance.target.transform.position - Instance.transform.position, 5, 1);
        Instance.Anim.SetTrigger("Ability");
        yield return new WaitForSeconds(1);
        Instance.SetState(new BigSlimeIdleState());
    }
    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        if (!Instance.target)
        {
            Instance.SetState(new BigSlimeIdleState());
            return;
        }
        attackCor = Instance.StartCoroutine(C_Attack());
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnUpdate()
    {
    }
}

public class BigSlimeHitState : IState<Enemy>
{
    public Enemy Instance { get; set; }
    IEnumerator C_Hit()
    {
        Instance.Anim.SetTrigger("Hit");
        yield return new WaitForSeconds(Instance.HitDelay);
        Instance.SetState(new BigSlimeIdleState());
    }
    public void OnEnter(Enemy instance)
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

public class BigSlimeDeathState : IState<Enemy>
{
    public Enemy Instance { get; set; }
    IEnumerator C_Death()
    {
        Instance.Anim.SetTrigger("Death");
        Instance.IsDeath = true;
        yield return new WaitForSeconds(0.5f);
        Object.Destroy(Instance.gameObject);
    }
    public void OnEnter(Enemy instance)
    {
        Instance = instance;
        Instance.StartCoroutine(C_Death());
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
    }
}
#endregion