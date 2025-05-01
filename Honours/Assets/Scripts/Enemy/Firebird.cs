using TMPro;
using System.Collections;
using UnityEngine;
using static Damage;
public class Firebird : MonoBehaviour, IDamageable
{
    Transform player;
    BoxCollider2D boxCollider2D;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    public Transform[] movementPoints;
    AudioSource flapAudioSource;

    public float pauseDuration = 1.5f; // Time spent at each position
    [SerializeField] float moveDuration = 2f; // Time to move between sides

    public GameObject projectilePrefab;
    public Transform firePoint;
    public float baseProjectileSpeed = 5f;
    public float fireRate = 2f; // Time between shots

    Vector2 targetPosition;
    public EnemyState currentState;
    int moveCount = 0; // Tracks number of moves

    public Room currentRoom;
    Coroutine firingCoroutine;
    bool isMoving = false;
    bool hasAppeared = false;
    int movesPerPhase = 2;

    bool isFrozen = false;
    float freezeTimer;
    bool isActive = true;

    // Enum for different phases of the boss
    public enum FirebirdPhase { Phase1, Phase2, Phase3 }
    public FirebirdPhase currentPhase = FirebirdPhase.Phase1;

    void Start()
    {
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (hasAppeared)
        {
            // Skip appearance wait and go straight into attacking
            StartCoroutine(BossRoutine());
        }
        else
        {
            // First-time appearance
            StartCoroutine(WaitForAppearanceAndStartRoutine());
        }

        if (DifficultyManager.Instance.IsHardMode())
        {
            movesPerPhase = -1;
            baseProjectileSpeed += 0.2f;
        }

        flapAudioSource = GetComponent<AudioSource>();

        if (flapAudioSource != null && !flapAudioSource.isPlaying)
        {
            flapAudioSource.Play();
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) { return; }
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void RestartBossRoutine()
    {
        StopAllCoroutines();
        hasAppeared = true;
        currentState = EnemyState.Attacking;
        StartCoroutine(BossRoutine());

        if (flapAudioSource != null && !flapAudioSource.isPlaying)
        {
            flapAudioSource.Play();
        }
    }

    public void Damage(int damage)
    {
        if (currentState == EnemyState.Dead) { return; }
        // Current health is decreased by the damage received
        BossEnemy enemy = GetComponentInParent<BossEnemy>();
        enemy.Damage(damage);
    }

    // Coroutine to wait for the appearance animation to finish before starting the boss routine
    IEnumerator WaitForAppearanceAndStartRoutine()
    {
        // Disable collider during appearance
        GetComponent<Collider2D>().enabled = false;

        if (!hasAppeared) // Only play "Appear" animation the first time
        {
            animator.Play("Appear"); // Play appear animation
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            hasAppeared = true; // Mark as appeared
        }

        // Enable collider and start the boss routine
        currentState = EnemyState.Attacking;
        boxCollider2D.enabled = true;
        StartCoroutine(BossRoutine());
    }

    // Updates animation direction based on velocity
    void UpdateAnimationDirection(Vector2 velocity)
    {
        if (velocity.sqrMagnitude > 0.01f) // Only update if there's movement
        {
            animator.SetFloat("velocityX", velocity.x);
            animator.SetFloat("velocityY", velocity.y);
        }

        animator.SetFloat("speed", velocity.magnitude); // Set speed for animation transitions
    }

    IEnumerator BossRoutine()
    {
        while (true)
        {
            foreach (var point in movementPoints)
            {
                targetPosition = point.position;
                yield return MoveToSetPosition(point.position);
                moveCount++;

                if (moveCount % movesPerPhase == 0 && currentPhase != FirebirdPhase.Phase3)
                {
                    currentPhase++;
                }

                firingCoroutine = StartCoroutine(FireAtPlayerRoutine());
                yield return new WaitForSeconds(pauseDuration);
                StopCoroutine(firingCoroutine);
            }
        }
    }


    IEnumerator MoveToSetPosition(Vector2 target)
    {
        isMoving = true;

        // Disable the collider while moving
        boxCollider2D.enabled = false;

        Vector2 startPosition = transform.position;

        // Calculate direction for animation
        Vector2 moveDirection = (target - startPosition).normalized;
        UpdateAnimationDirection(moveDirection);

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(startPosition, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        // Enable the collider once movement is finished
        boxCollider2D.enabled = true;

        isMoving = false;

        // Stop movement animation when reaching the position
        UpdateAnimationDirection(Vector2.zero);
    }

    void FireSpreadAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null || isFrozen) return;

        int numberOfProjectiles;
        float spreadAngle;
        float projectileSpeed = baseProjectileSpeed;

        // Adjust fire pattern based on phase
        switch (currentPhase)
        {
            case FirebirdPhase.Phase1:
                numberOfProjectiles = 3;
                spreadAngle = 20f;
                break;
            case FirebirdPhase.Phase2:
                numberOfProjectiles = 5;
                spreadAngle = 30f;
                projectileSpeed *= 1f;
                break;
            case FirebirdPhase.Phase3:
                numberOfProjectiles = 7;
                spreadAngle = 45f;
                projectileSpeed *= 1.2f;
                break;
            default:
                numberOfProjectiles = 3;
                spreadAngle = 20f;
                break;
        }

        float angleStep = spreadAngle / (numberOfProjectiles - 1);
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = -spreadAngle / 2 + angleStep * i;
            Vector2 fireDirection = (player.position - firePoint.position).normalized;
            fireDirection = RotateVector(fireDirection, angle);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.velocity = fireDirection * projectileSpeed;
            }
        }
    }

    // Helper function to rotate a vector by a given angle
    Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y);
    }

    IEnumerator FireAtPlayerRoutine()
    {
        // Ensure the Firebird is facing the player
        UpdateAnimationDirectionTowardsPlayer();
        while (true)
        {
            // Adjust chance to fire flare scatter pattern based on difficulty
            float flareChance = 0.1f;
            if (DifficultyManager.Instance.IsHardMode())
            {
                flareChance = 0.3f; // Increased chance on hard mode
            }

            // Decide if firebird should fire the FlareScatter
            if (Random.value < flareChance)
            {
                FlareScatter();
            }
            else
            {
                // Otherwise, pick one of the other attacks to fire
                int attackType = Random.Range(0, 2);
                if (attackType == 0)
                {
                    FireSpreadAtPlayer();
                }
                else
                {
                    FireRapidShots();
                }
            }

            // Wait for the next attack cycle
            yield return new WaitForSeconds(fireRate);
        }
    }



    void FireRapidShots()
    {
        for (int i = 0; i < 6; i++)
        {
            StartCoroutine(FireDelayedProjectile(i * 0.2f));
        }
    }

    IEnumerator FireDelayedProjectile(float delay)
    {
        yield return new WaitForSeconds(delay);
        FireProjectile((player.position - firePoint.position).normalized);
    }

    void FireProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fireBall);
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = direction * baseProjectileSpeed;
        }
    }

    void FlareScatter()
    {
        int projectileCount = 15;
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float randomOffset = Random.Range(-10f, 10f);
            float angle = i * angleStep + randomOffset;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            FireProjectile(dir);
        }
    }


    // Update the animation direction to face the player when the Firebird stops moving
    void UpdateAnimationDirectionTowardsPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        animator.SetFloat("velocityX", directionToPlayer.x);
        animator.SetFloat("velocityY", directionToPlayer.y);
        animator.SetFloat("speed", 0f); // Stop the movement animation when still
    }

    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            isFrozen = true;
            freezeTimer = duration;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.iceHitEffect);

            // Stop movement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePosition;
            }

            // Freeze the animator by setting its speed to 0
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0;
            }

            // Set colour to cyan to indicate frozen state
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.cyan;
            }

            Invoke("Unfreeze", freezeTimer);
        }
    }

    void Unfreeze()
    {
        isFrozen = false;
        isActive = true;

        // Restore the colour to white to indicate the enemy is no longer frozen
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        // Restore animator speed
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 1;
        }

        // Remove position constraints 
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
