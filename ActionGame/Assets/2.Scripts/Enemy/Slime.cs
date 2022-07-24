using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    public override void Hit(int damage)
    {
        Hp -= damage;
        SetState(new SlimeHitState());
    }

    protected override void Awake()
    {
        base.Awake();
        DeathAction += () => SetState(new SlimeDeathState());
    }
    protected override void Start()
    {
        SetState(new SlimeIdleState());
    }
}

#region FSM
public class SlimeIdleState : IState<Enemy>
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
            Instance.SetState(new SlimeMoveState());
        }
    }
}

public class SlimeMoveState : IState<Enemy>
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
            Instance.SetState(new SlimeIdleState()); //Idle�� ���� ( �̵� �ߴ� ) 
        if (dist < 0.5f) //�Ÿ��� 0.5f �̸��̸�
            Instance.SetState(new SlimeAttackState()); //���ݻ��·� ����

        //Ÿ���� ���� �̵�
        Vector3 dir = (Instance.target.transform.position - Instance.transform.position).normalized;
        Instance.transform.position += dir * Instance.MoveSpeed * Time.deltaTime;
        if (dir.x != 0)
            Instance.m_SpriteRenderer.flipX = dir.x < 0;
    }
}

public class SlimeAttackState : IState<Enemy>
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
        Instance.SetState(new SlimeIdleState()); //Idle���·� ����
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

public class SlimeHitState : IState<Enemy>
{
    public Enemy Instance { get; set; }
    IEnumerator C_Hit()
    {
        Instance.Anim.SetTrigger("Hit");
        yield return new WaitForSeconds(Instance.HitDelay);
        Instance.SetState(new SlimeIdleState());
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
public class SlimeDeathState : IState<Enemy>
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