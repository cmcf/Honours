using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Enemy
{
    Transform player;
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform[] firePoints;

    [SerializeField] float attackRadius = 10f;
    
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackPause = 2f;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float minDistance = 2f; // Prevents getting too close to the player
    [SerializeField] float strafeDistance = 3f; // Distance for side movement

    [SerializeField] float minSpeed = 4f;
    [SerializeField] float maxSpeed = 6.5f;

    float speed;

    bool lastAttackHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        speed = Random.Range(minSpeed, maxSpeed);
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Stop movement when hitting a wall
            rb.velocity = Vector2.zero;
        }
    }

    void AdjustMovement()
    {
        animator.SetBool("isMoving", true);
        Vector2 direction = (player.position - (Vector3)transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If too close, move sideways instead of forward
        if (distanceToPlayer <= minDistance)
        {
            Vector2 strafeDirection = (Random.value > 0.5f) ? Vector2.left : Vector2.right;
            direction = strafeDirection;
        }

        // Prioritise moving below the player
        if (transform.position.y > player.position.y)
        {
            direction = Vector2.down;
        }

        // Use MovePosition for smooth movement
        Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPosition);
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
        lastAttackHit = true; 
    }

    IEnumerator MoveToBetterPosition()
    {
        if (player == null) yield break;

        Vector2 targetPosition = (Vector2)transform.position + ((Vector2)player.position - (Vector2)transform.position).normalized * 2f;

        // Prioritise moving below the player
        if (transform.position.y > player.position.y)
        {
            targetPosition = new Vector2(transform.position.x, player.position.y - 2f);
        }

        animator.SetBool("isMoving", true); // Start move animation

        while ((Vector2)transform.position != targetPosition)
        {
            // Move smoothly like AdjustMovement
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime);

            // Stop if colliding with a wall
            if (Physics2D.OverlapCircle(newPosition, 0.2f, LayerMask.GetMask("Wall")) != null)
            {
                break;
            }

            rb.MovePosition(newPosition);
            yield return null;
        }

        animator.SetBool("isMoving", false); // Stop move animation
    }

}
