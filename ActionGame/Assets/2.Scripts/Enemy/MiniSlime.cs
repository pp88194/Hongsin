using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MiniSlime : MonoBehaviour
{
    Vector2 dir;
    int attackDamage;
    float speed;
    Coroutine moveCor;
    public void Init(Vector2 dir,float speed, int damage)
    {
        this.dir = dir.normalized;
        this.speed = speed;
        this.attackDamage = damage;
        moveCor = StartCoroutine(C_Move());
    }
    IEnumerator C_Move()
    {
        for(float t = 0; t < 3; t += Time.fixedDeltaTime)
        {
            transform.position += (Vector3)dir * speed * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.GetComponent<Player>().Hit(attackDamage);
            StopCoroutine(C_Move());
            Destroy(gameObject);
        }
    }
}