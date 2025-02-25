using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Panther : MonoBehaviour, IDamageable
{
    Transform player;
    Animator animator;
    Rigidbody2D rb;

    // Melee Attack Variables
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    bool canAttack = true;

    public EnemyState currentState;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (canAttack)
        {
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                StartCoroutine(MeleeAttack());
            }
        }
    }

    IEnumerator MeleeAttack()
    {
        canAttack = false;
        animator.SetTrigger("Attack"); 
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void Damage(float damage)
    {
        if (currentState == EnemyState.Dead) { return; }
        // Current health is decreased by the damage received
        BossEnemy enemy = GetComponentInParent<BossEnemy>();
        enemy.Damage(damage);
    }
}
