using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Shell : Enemy
{
    Transform player;
    [SerializeField] GameObject spikePrefab;
    [SerializeField] Transform[] firePoints;

    [SerializeField] float attackRadius = 10f;
    [SerializeField] int hitDamageAmount = 5;
    
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackPause = 2f;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float minDistance = 2f; // Prevents getting too close to the player
    [SerializeField] float strafeDistance = 3f; // Distance for side movement

    [SerializeField] float minSpeed = 4f;
    [SerializeField] float maxSpeed = 6.5f;

    public float speed;
    float waitTimeBeforeMoveCheck = 1.5f;
    bool lastAttackHit = false;
    bool lastAttackHitPreviously = false;

    bool isHardMode;
    bool hitPlayer = false;

    FirePattern lastUsedPattern;
    enum FirePattern
    {
        Blast,
        Multiple
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        speed = Random.Range(minSpeed, maxSpeed);

        isHardMode = DifficultyManager.Instance.IsHardMode();

        if (isHardMode)
        {
            attackPause -= 0.2f;
        }

    }

    void Update()
    {
        if (!isActive) { return; }; 
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
            AdjustMovement(); // Move  when not attacking
        }

    }


    void FlipSprite()
    {
        if (player != null)
        {
            float directionX = player.position.x - transform.position.x;
            Vector3 scale = transform.localScale;
            scale.x = directionX > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
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
        FirePattern firePattern;

        // Repeat the last attack if the previous one hit
        if (lastAttackHitPreviously && currentState == EnemyState.Attacking)
        {
            firePattern = lastUsedPattern;
        }
        else
        {
            firePattern = GetRandomFirePattern();
            lastUsedPattern = firePattern;
        }

        // Stop moving while firing
        rb.velocity = Vector2.zero; // Ensure the shell enemy stops moving

        // Execute the fire pattern
        switch (firePattern)
        {
            case FirePattern.Blast:
                StartCoroutine(OneShotBlast());  // One-shot blast from all points
                break;
            case FirePattern.Multiple:
                StartCoroutine(FireFromMultiplePoints());  // Fire from multiple points
                break;
        }

        StartCoroutine(WaitAndCheckHit());
    }


    FirePattern GetRandomFirePattern()
    {
        // Randomly select one of the fire patterns
        int randomValue = Random.Range(0, 3);  
        switch (randomValue)
        {
            case 0:
                return FirePattern.Multiple;
            case 1:
                return FirePattern.Blast;
            default:
                return FirePattern.Multiple;  // Default 
        }
    }


    IEnumerator OneShotBlast()
    {
        // Fire all projectiles at once from all fire points
        foreach (Transform firePoint in firePoints)
        {
            GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = firePoint.up * projectileSpeed;
            }

            Spike spikeScript = spike.GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetShellOwner(this);
            }
        }

        yield return new WaitForSeconds(attackPause); // Wait for attack pause after firing
    }


    // Fires from a random selection of fire points 
    IEnumerator FireFromMultiplePoints()
    {
        // Random number of fire points to use 
        int firePointsToUse = Random.Range(2, firePoints.Length + 1);

        // Randomly pick firePointsToUse fire points
        List<Transform> selectedFirePoints = new List<Transform>();
        while (selectedFirePoints.Count < firePointsToUse)
        {
            Transform randomFirePoint = firePoints[Random.Range(0, firePoints.Length)];
            if (!selectedFirePoints.Contains(randomFirePoint))
            {
                selectedFirePoints.Add(randomFirePoint);
            }
        }

        // Fire from each selected fire point
        foreach (Transform firePoint in selectedFirePoints)
        {
            GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                // Set off set based on difficulty
                float angleOffset;
                if (isHardMode)
                {
                    angleOffset = Random.Range(-2f, 2f);
                }
                else
                {
                    angleOffset = Random.Range(-5f, 5f);
                }

                direction = Quaternion.Euler(0, 0, angleOffset) * direction;
                rb.velocity = direction * projectileSpeed;
            }


            Spike spikeScript = spike.GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetShellOwner(this);
            }

            float waitBeforeNextProjectile;

            if (isHardMode)
            {
                waitBeforeNextProjectile = 0.1f;
            }
            else
            {
                waitBeforeNextProjectile = 0.2f;
            }

            yield return new WaitForSeconds(waitBeforeNextProjectile);
        }
    }



    IEnumerator WaitAndCheckHit()
    {
        yield return new WaitForSeconds(waitTimeBeforeMoveCheck);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!lastAttackHit && distanceToPlayer <= attackRadius)
        {
            // Shell can pick another attack pattern if missed
            lastAttackHitPreviously = false;
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
        lastAttackHitPreviously = true;
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
            // Exit early if player moves out of range
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deals damage to player if hit
            hitPlayer = true;
            DealDamage(collision.transform);
            Destroy(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        hitPlayer = false;
    }

    void DealDamage(Transform target)
    {
        // Check if the target has a damageable component
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Deal damage to the player
            damageable.Damage(hitDamageAmount);
        }
    }
}
    



