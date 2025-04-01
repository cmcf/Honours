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

    public float speed;

    bool lastAttackHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        FlipSprite(); // Always face the player

        float distance = Vector2.Distance(transform.position, player.position);

        if (currentState == EnemyState.Attacking)
        {
            rb.velocity = Vector2.zero; 
            return;
        }

        if (distance <= attackRadius)
        {
            Attack();
        }
        else
        {
            AdjustMovement(); // Move smoothly when not attacking
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

        Vector2 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If too close, don't move forward
        if (distanceToPlayer <= minDistance)
        {
            rb.velocity = Vector2.zero; // Stop moving if too close
            return;
        }

        // Move smoothly towards the player
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

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!lastAttackHit && distanceToPlayer <= attackRadius) // Missed & player still in range
        {
            StartCoroutine(MoveToBetterPosition());
        }
        else
        {
            currentState = EnemyState.Idle; // Stop adjusting if player left range
        }
    }




    public void RegisterHit()
    {
        lastAttackHit = true; 
    }

    IEnumerator MoveToBetterPosition()
    {
        if (player == null) yield break;

        Vector2 targetPosition = (Vector2)transform.position + ((Vector2)player.position - (Vector2)transform.position).normalized * speed;

        // Prioritise moving below the player
        if (transform.position.y > player.position.y)
        {
            targetPosition = new Vector2(transform.position.x, player.position.y - 2f);
        }

        animator.SetBool("isMoving", true); // Start move animation

        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            // **New Fix:** Exit early if player moves out of range
            if (Vector2.Distance(transform.position, player.position) > attackRadius)
            {
                animator.SetBool("isMoving", false);
                yield break; // Stop adjusting, return to normal movement
            }

            // Move smoothly
            Vector2 direction = (targetPosition - rb.position).normalized;
            rb.velocity = direction * speed;

            yield return null;
        }

        rb.velocity = Vector2.zero; // Stop once at target
        animator.SetBool("isMoving", false);
    }


}
