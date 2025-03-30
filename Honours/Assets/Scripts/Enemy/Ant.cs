using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : Enemy
{
    Transform player;
    [SerializeField] float speed = 3f;
    [SerializeField] float attackRadius = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackDuration = 0.35f;

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

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed; 

        // If the ant is moving, trigger the walking animation
        if (rb.velocity.magnitude > 0.1f) 
        {
            animator.SetBool("isMoving", true); // Trigger walking animation
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop movement when not moving
            animator.SetBool("isMoving", false); // Prevent walking animation from playing
        }

    }

    IEnumerator Attack()
    {
        currentState = EnemyState.Attacking; 
        rb.velocity = Vector2.zero; // Stop any movement during attack
        animator.SetBool("isMoving", false); // Stop moving animation
        animator.SetBool("isAttacking", true); // Start attack animation

        yield return new WaitForSeconds(attackDuration); // Wait for attack duration

        animator.SetBool("isAttacking", false); // Stop attack animation

        // Pause for attack cooldown before moving again
        yield return new WaitForSeconds(attackCooldown);

        currentState = EnemyState.Idle; // Allow attacking again

        // When attack is done, if ant is not moving, make sure walk animation is not triggered
        if (rb.velocity.magnitude == 0f)
        {
            animator.SetBool("isMoving", false);
        }
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
}
