using TMPro;
using System.Collections;
using UnityEngine;
using static Damage;
public class Firebird : MonoBehaviour, IDamageable
{
    Transform player;
    BoxCollider2D boxCollider2D;
    Animator animator;
    Rigidbody2D rb;
    public Transform[] movementPoints;

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

    // Enum for different phases of the boss
    public enum FirebirdPhase { Phase1, Phase2, Phase3 }
    public FirebirdPhase currentPhase = FirebirdPhase.Phase1;

    void Start()
    {
        // References 
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();

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
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void RestartBossRoutine()
    {
        StopAllCoroutines();
        hasAppeared = true;  
        StartCoroutine(BossRoutine());
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

                if (moveCount % 2 == 0 && currentPhase != FirebirdPhase.Phase3)
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
        if (projectilePrefab == null || firePoint == null) return;

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

    // Coroutine to fire at the player
    IEnumerator FireAtPlayerRoutine()
    {

        // Ensure the Firebird is facing the player
        UpdateAnimationDirectionTowardsPlayer();
        while (true)
        {
            int attackType = Random.Range(0, 2);
            if (attackType == 0)
                FireSpreadAtPlayer();
            else
                FireRapidShots();
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
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = direction * baseProjectileSpeed;
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
}
