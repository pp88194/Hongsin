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
        //�ֺ��� �÷��̾ �ִ��� ����
        RaycastHit2D hit = Physics2D.CircleCast(Instance.transform.position, 5, Vector2.zero, 5, LayerMask.GetMask("Player"));
        if (hit)
        { //�÷��̾ �����Ǿ����� �÷��̾ Ÿ������ �����ϰ� �̵����·� ����
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

        //dist = �ڽŰ� Ÿ��(�÷��̾�)���� �Ÿ�
        float dist = Vector2.Distance(Instance.transform.position, Instance.target.transform.position);
        if (!Instance.target || dist > 5) //Ÿ���� null�̰ų� �Ÿ��� 5�̻��̸�
            Instance.SetState(new BigSlimeIdleState()); //Idle�� ���� ( �̵� �ߴ� ) 
        if (rangeAttackDelay > 5f) //���Ÿ����ݴ��ð��� �� ���� ���Ÿ�����
        {
            Instance.SetState(new BigSlimeRangeAttack());
            rangeAttackDelay = 0;
        }
        if (dist < 0.5f) //�Ÿ��� 0.5f �̸��̸�
            Instance.SetState(new BigSlimeMeleeAttackState()); //���ݻ��·� ����

        //Ÿ���� ���� �̵�
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
         *  [ ��� ��� ���� ���� ]
         * if(Instance.target != null) {
         *   Instance.target.SetState();
         * }
         */
        Instance.target?.Hit(Instance.AttackDamage); //target�� null�� �ƴϸ� target�� ���¸� �ǰݻ��·� ����
        yield return new WaitForSeconds(1); //1�� ���
        Instance.SetState(new BigSlimeIdleState()); //Idle���·� ����
    }
    public virtual void OnEnter(Enemy instance)
    {
        Instance = instance;
        attackCor = Instance.StartCoroutine(C_Attack()); //�ڷ�ƾ ����
        Instance.Anim.SetTrigger("Attack");
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