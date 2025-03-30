using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : Enemy
{
    Transform player;
    [SerializeField] float speed = 3f;
    [SerializeField] float attackRadius = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackDuration = 0.5f;

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        FlipSprite();

        if (distance <= attackRadius)
        {
            if (currentState != EnemyState.Attacking)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (currentState == EnemyState.Attacking) return; 

        animator.SetBool("isAttacking", false);
        animator.SetBool("isMoving", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }

    void FlipSprite()
    {
        if (player == null) return;

        float directionX = player.position.x - transform.position.x;
        if (directionX > 0)
            transform.localScale = new Vector3(-1, 1, 1); // Face right
        else if (directionX < 0)
            transform.localScale = new Vector3(1, 1, 1);  // Face left
    }

    IEnumerator Attack()
    {
        currentState = EnemyState.Attacking;

        while (Vector2.Distance(transform.position, player.position) <= attackRadius)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);

            yield return new WaitForSeconds(attackDuration); 

            animator.SetBool("isAttacking", false);
            yield return new WaitForSeconds(attackCooldown); // Pause before next attack
        }

        // When the player leaves the attack radius
        currentState = EnemyState.Idle;
    }
}
