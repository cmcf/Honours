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
    [SerializeField] float minDistance = 2f; // Prevents getting too close to the player
    [SerializeField] float strafeDistance = 3f; // Distance for side movement

    bool lastAttackHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null)
        {
            FlipSprite(); // Always face the player

            if (currentState != EnemyState.Attacking)
            {
                float distance = Vector2.Distance(transform.position, player.position);

                if (distance <= attackRadius)
                {
                    Attack();
                }
                else
                {
                    AdjustMovement();
                }
            }
        }
    }

    void FlipSprite()
    {
        if (player != null)
        {
            float directionX = player.position.x - transform.position.x;
            if (directionX > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (directionX < 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void AdjustMovement()
    {
        animator.SetBool("isMoving", true);
        Vector2 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If too close, move sideways instead of forward
        if (distanceToPlayer <= minDistance)
        {
            // Randomly move left or right
            Vector2 strafeDirection = (Random.value > 0.5f) ? Vector2.left : Vector2.right;
            direction = strafeDirection;
        }

        // Prioritise moving below the player
        if (transform.position.y > player.position.y)
        {
            direction = Vector2.down; // Move downward
        }

        rb.velocity = direction * speed;
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
        lastAttackHit = false; // Reset before firing

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != transform)
            {
                GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
                Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.velocity = firePoint.up * projectileSpeed; // Fire in correct direction
                }

                Spike spikeScript = spike.GetComponent<Spike>();
                if (spikeScript != null)
                {
                    spikeScript.SetShellOwner(this);
                }
            }
        }

        StartCoroutine(WaitAndCheckHit());
    }

    IEnumerator WaitAndCheckHit()
    {
        yield return new WaitForSeconds(1f); // Wait for spikes to reach target

        if (!lastAttackHit) // If the attack missed
        {
            StartCoroutine(MoveToBetterPosition());
        }
    }

    public void RegisterHit()
    {
        lastAttackHit = true; // Attack was successful
    }

    IEnumerator MoveToBetterPosition()
    {
        if (player == null) yield break;

        Vector2 start = transform.position;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 targetPosition = start + (directionToPlayer * 2f);

        // Prioritise moving below the player
        if (transform.position.y > player.position.y)
        {
            targetPosition = new Vector2(transform.position.x, player.position.y - 2f);
        }

        float elapsedTime = 0f;
        float moveDuration = 0.5f; 

        animator.SetBool("isMoving", true); // Start move animation

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(start, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        animator.SetBool("isMoving", false); // Stop move animation
    }
}
