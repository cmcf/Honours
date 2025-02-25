using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panther : MonoBehaviour
{
    Transform player;
    Animator animator;
    Rigidbody2D rb;

    // Melee Attack Variables
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    bool canAttack = true;

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

        // You could also add movement logic or behavior here for the panther
    }

    IEnumerator MeleeAttack()
    {
        canAttack = false;
        animator.SetTrigger("Attack"); // Assume you have an attack animation
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
