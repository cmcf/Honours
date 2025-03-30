using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Enemy
{
    Transform player;
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform[] firePoints;

    [SerializeField] float attackRadius = 10f;
    [SerializeField] float speed = 8f;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackPause = 2f;
    [SerializeField] float projectileSpeed = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null && currentState != EnemyState.Attacking)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= attackRadius)
            {
                Attack();
            }
            else
            {
                FollowPlayer();
            }
        }
    }

    void FollowPlayer()
    {
        animator.SetBool("isMoving", true);
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;

        FlipSprite(direction.x); // Flip the sprite
    }

    void FlipSprite(float directionX)
    {
        if (directionX > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (directionX < 0)
            transform.localScale = new Vector3(1, 1, 1);
    }

    void Attack()
    {
        if (currentState != EnemyState.Attacking)
        {
            currentState = EnemyState.Attacking;
            rb.velocity = Vector2.zero; // Stop moving when attacking
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
            StartCoroutine(AttackCooldown());
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown); // Wait for cooldown
        animator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(attackPause); // Pause before next attack

        currentState = EnemyState.Idle; // Allow attacking again
    }

    public void FireSpikes()
    {
        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != transform) 
            {
                GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
                Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.velocity = firePoint.up * projectileSpeed;
                }
            }
        }
    }

}
