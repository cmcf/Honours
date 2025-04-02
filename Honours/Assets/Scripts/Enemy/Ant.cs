using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : Enemy
{
    [Header("References")]
    Transform player;

    [Header("Speed")]
    [SerializeField] float minSpeed = 3f;
    [SerializeField] float maxSpeed = 6f;
    float speed;

    [Header("Attack")]
    [SerializeField] float attackRadius = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackDuration = 0.35f;

    [Header("Projectile")]
    [SerializeField] GameObject projectilePrefab; 
    [SerializeField] Transform projectileSpawnPoint; 
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] float projectileLife = 1.5f;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
    }

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

        animator.SetBool("isMoving", rb.velocity.magnitude > 0.1f);
    }

    IEnumerator Attack()
    {
        currentState = EnemyState.Attacking;
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", true);

        yield return new WaitForSeconds(attackDuration);

        // Fire a projectile directly at the player's position at attack time
        FireProjectile();

        animator.SetBool("isAttacking", false);

        yield return new WaitForSeconds(attackCooldown); 

        currentState = EnemyState.Idle;
    }

    void FireProjectile()
    {
        if (projectilePrefab && projectileSpawnPoint && player)
        {
            // Instantiate the projectile at the correct fire point
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

            // Calculate direction immediately
            Vector2 direction = (player.position - projectile.transform.position).normalized;

            // Rotate projectile to face movement direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Apply velocity 
            projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;

            // Destroy projectile
            Destroy(projectile, projectileLife);
        }
    }


    void FlipSprite()
    {
        if (player == null) return;
        float directionX = player.position.x - transform.position.x;
        transform.localScale = new Vector3(directionX > 0 ? -1 : 1, 1, 1);
    }
}
