using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;
using Random = UnityEngine.Random;

public class Panther : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Transform[] boundaryPoints;
    Animator animator;
    Rigidbody2D rb;
    public Room currentRoom;
    public Transform target;
    TrailRenderer trailRenderer;

    [Header("Charge")]
    [SerializeField] float chargeSpeed = 8f;
    [SerializeField] float chargeCooldown = 5f;
    [SerializeField] float chargeDuration = 1f;
    [SerializeField] float attackSpeed = 2f;
    float chargeStartTime;
    bool isCharging = false;

    [Header("Melee")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float stopAttackRange = 4f;
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackCooldown = 1f;

    bool isAttacking = false;
    bool canAttackAgain = true;

    [Header("Shield")]
    public GameObject shieldPrefab;
    public Transform shieldSpawnPoint;
    List<GameObject> activeShields = new List<GameObject>();
    public bool shieldActive = false;
    [SerializeField] float minShieldDuration = 5.5f;
    [SerializeField] float maxShieldDuration = 8.5f;
    [SerializeField] float orbitRadius = 0.5f;
    [SerializeField] float orbitSpeed = 100f; // Degrees per second
    [SerializeField] float projectileSpeed = 6.5f; // Speed of the fired shield projectiles
    [SerializeField] float respawnShieldDelay = 2f;
    [SerializeField] float defendStateDuration = 25f;
    [SerializeField] float vulnerabilityTime = 8f;
    [SerializeField] float shieldCooldown = 2f;
    [SerializeField] bool canActivateShield = true;
    [SerializeField] float shieldCount = 5;
    float shieldDuration;
    
    bool shieldScheduledThisPhase = false;
    float lastShieldTime;
    float defendStateStartTime;
    Vector2 chargeDirection; // Stores the direction during the charge

    public EnemyState currentState;
    public enum PantherState { Defend, Charge, Attack }
    public PantherState currentPhase = PantherState.Defend;

    int difficultyLevel;
    bool isHardMode;

    void Start()
    {
        // References to components
        player = GameObject.FindGameObjectWithTag("Player").transform;
        shieldDuration = Random.Range(minShieldDuration, maxShieldDuration);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();

        // Get current difficulty
        difficultyLevel = DifficultyManager.Instance.currentDifficulty;
        isHardMode = DifficultyManager.Instance.IsHardMode();
        ApplyDifficultySettings();

    }

    void ApplyDifficultySettings()
    {
        if (difficultyLevel >= 4)
        {
            chargeSpeed += 2f;
            chargeCooldown -= 1f;

            attackDamage += 2;
            attackCooldown -= 0.25f;

            projectileSpeed += 1.5f;
            orbitSpeed += 20f;
            shieldCount += 1;

            minShieldDuration -= 1f;
            maxShieldDuration -= 1.5f;
        }

        if (isHardMode)
        {
            chargeSpeed += 2.5f;
            chargeCooldown -= 1f;

            attackDamage += 4;
            attackCooldown -= 0.25f;

            projectileSpeed += 2f;
            orbitSpeed += 40f;
            shieldCount += 1;

            minShieldDuration -= 1.5f;
            maxShieldDuration -= 2f;
        }

        // Clamp to sensible values
        chargeCooldown = Mathf.Max(1f, chargeCooldown);
        attackCooldown = Mathf.Max(0.3f, attackCooldown);
        minShieldDuration = Mathf.Max(1f, minShieldDuration);
        maxShieldDuration = Mathf.Max(minShieldDuration + 0.5f, maxShieldDuration);

        shieldDuration = Random.Range(minShieldDuration, maxShieldDuration);
    }

    void OnEnable()
    {
        canActivateShield = true;
        defendStateStartTime = Time.time;
        currentPhase = PantherState.Defend;
    }

    float nextDashTime;

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        FacePlayer();

        if (shieldActive)
        {
            rb.velocity = Vector2.zero;
            UpdateShieldOrbit();
            return;
        }

        if (currentPhase != PantherState.Charge)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            // If vulnerable and in range, try melee
            if (distToPlayer <= attackRange && canAttackAgain)
            {
                AttackPlayer();
            }
            // Otherwise, dash occasionally if shield is down
            else if (Time.time >= nextDashTime && !isCharging)
            {
                // Only dash if the shield is down and panther has been idle for a bit
                if (!shieldActive && Time.time - defendStateStartTime >= shieldCooldown)
                {
                    StartChargePhase();
                    nextDashTime = Time.time + chargeCooldown; // Set next dash time
                }
            }

            // Start shield again after defend duration
            if (canActivateShield && !shieldActive && !shieldScheduledThisPhase && Time.time - defendStateStartTime >= shieldCooldown)
            {
                ActivateShield();
                shieldScheduledThisPhase = true;
            }
        }

        animator.SetFloat("speed", rb.velocity.magnitude);
    }



    public void StartAttacking()
    {
        if (!isCharging)
        {
            StartChargePhase();
        }
    }

    void StartChargePhase()
    {
        if (isCharging) return;

        isCharging = true;
        chargeStartTime = Time.time;

        chargeDirection = (player.position - transform.position).normalized;
        animator.SetFloat("speed", chargeSpeed);
        animator.SetFloat("posX", chargeDirection.y);
        animator.SetFloat("posY", chargeDirection.x);
        trailRenderer.emitting = true;

        StartCoroutine(PerformCharge());
    }


    IEnumerator PerformCharge()
    {
        float elapsed = 0f;
        bool hasHitPlayer = false;  // Flag to check if player has already been hit

        // Apply dash movement while checking for wall collisions
        while (elapsed < chargeDuration)
        {
            // Stop if panther hit a wall
            RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDirection, 0.5f);
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                rb.velocity = Vector2.zero; // Stop panther when hitting wall
                animator.SetFloat("speed", 0);
                break; // Exit the charge when hitting the wall
            }

            // Apply velocity to the Panther for dashing
            rb.velocity = chargeDirection * chargeSpeed;

            // Check for collision with player during dash
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (Collider2D h in hits)
            {
                if (h.CompareTag("Player") && !hasHitPlayer) // Check if not already hit
                {
                    IDamageable damageable = h.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.Damage(attackDamage); // Apply damage only once
                        hasHitPlayer = true; // Set flag to true after hitting player
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // End of dash
        rb.velocity = Vector2.zero;
        animator.SetFloat("speed", 0);
        isCharging = false;
        trailRenderer.emitting = false;
        currentPhase = PantherState.Defend; // Transition to Defend state
        defendStateStartTime = Time.time; // Reset defend timer

        // Activate shield after dash finishes
        ActivateShield();
    }


    void AttackPlayer()
    {
        if (!isAttacking && canAttackAgain)
        {
            currentPhase = PantherState.Attack;
            isAttacking = true;
            animator.SetBool("isAttacking", true);

            // Prevent further attacks for a short duration
            canAttackAgain = false;

            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        float attackElapsed = 0f;

        // Perform the attack for the set cooldown time
        while (attackElapsed < attackCooldown)
        {
            attackElapsed += Time.deltaTime;

            // If the player moves out of range during the attack, cancel attack
            if (Vector2.Distance(transform.position, player.position) > attackRange)
            {
                StopAttack();
                yield break;
            }

            yield return null;
        }

        StopAttack();
    }

    void StopAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        rb.velocity = Vector2.zero; // Stop any movement during attack
        animator.SetFloat("speed", 0);

        // Reset attack cooldown and allow another attack after the cooldown
        canAttackAgain = true;
    }


    void AttackDamage()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D obj in hitObjects)
        {
            IDamageable damageable = obj.GetComponent<IDamageable>();

            if (damageable != null && obj.CompareTag("Player"))
            {
                damageable.Damage(attackDamage);
            }
        }
    }

    public void ActivateShield()
    {
        if (shieldActive || Time.time < lastShieldTime + shieldCooldown) return;

        // Prevent reactivating shield
        lastShieldTime = Time.time;

        // If shields are already active, stop the current ones first
        DeactivateShields();

        // Instantiate new shields around the Panther
        float angleStep = 360f / shieldCount;
        for (int i = 0; i < shieldCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 shieldPos = new Vector3(
                transform.position.x + Mathf.Cos(angle) * orbitRadius,
                transform.position.y + Mathf.Sin(angle) * orbitRadius,
                0f
            );
            GameObject shield = Instantiate(shieldPrefab, shieldPos, Quaternion.identity, transform);
            activeShields.Add(shield);
        }

        shieldActive = true;

        // Start the shield orbiting
        StartCoroutine(UpdateShieldOrbit());

        // Start firing projectiles from shields
        StartCoroutine(FireProjectilesFromShields());

        // Set a timer to deactivate shield after duration
        StartCoroutine(EndShieldAfterDuration());
    }

    void DeactivateShields()
    {
        // Remove all active shields and stop them from orbiting
        foreach (GameObject shield in activeShields)
        {
            if (shield != null)
            {
                Destroy(shield); // Destroy the shield objects
            }
        }
        activeShields.Clear(); // Clear the list of active shields
        shieldActive = false;
    }

    IEnumerator EndShieldAfterDuration()
    {
        // Wait for the specified duration of the shield
        yield return new WaitForSeconds(shieldDuration);

        // Deactivate the shield after the duration
        DeactivateShields();

        // Transition to Defend state
        currentPhase = PantherState.Defend;
        defendStateStartTime = Time.time; // Reset defend timer
    }

    IEnumerator UpdateShieldOrbit()
    {
        // Keep updating the orbit positions while the shield is active
        while (shieldActive)
        {
            for (int i = 0; i < activeShields.Count; i++)
            {
                if (activeShields[i] == null) continue;

                // Update the shield's orbit position
                float angle = (Time.time * orbitSpeed + (360f / activeShields.Count) * i) * Mathf.Deg2Rad;
                Vector3 shieldPos = new Vector3(
                    transform.position.x + Mathf.Cos(angle) * orbitRadius,
                    transform.position.y + Mathf.Sin(angle) * orbitRadius,
                    0f
                );

                activeShields[i].transform.position = shieldPos;
            }

            yield return null;
        }
    }

    IEnumerator FireProjectilesFromShields()
    {
        while (shieldActive)
        {
            // Decide how many shields will fire and which shields will fire
            int shieldsToFire = Random.Range(1, activeShields.Count + 1); // Randomly pick how many shields to fire
            List<int> selectedShields = new List<int>(); // Keeps track of which shields are selected

            // Randomly select shields to fire from
            while (selectedShields.Count < shieldsToFire)
            {
                int shieldIndex = Random.Range(0, activeShields.Count);
                if (!selectedShields.Contains(shieldIndex))
                {
                    selectedShields.Add(shieldIndex);
                }
            }

            // Fire projectiles from the selected shields
            foreach (int shieldIndex in selectedShields)
            {
                GameObject shield = activeShields[shieldIndex];
                if (shield != null)
                {
                    Vector3 spawnPosition = shield.transform.position;
                    FireProjectile(spawnPosition);
                }
            }

            // Delay before the next round of firing
            yield return new WaitForSeconds(Random.Range(0.5f, 2.5f)); // Random delay between shots
        }
    }

    void FireProjectile(Vector3 spawnPosition)
    {
        GameObject projectile = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);

        // Calculate direction towards the player
        Vector2 direction = (player.position - spawnPosition).normalized;

        // Set the projectile's velocity to move it towards the player
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        // Destroy the projectile after some time
        Destroy(projectile, 5f);
    }



    public void OnShieldDestroyed(GameObject destroyedShield)
    {
        canActivateShield = false;
        shieldScheduledThisPhase = false;
        activeShields.Remove(destroyedShield);

        if (activeShields.Count == 0)
        {
            shieldActive = false;

            // Add a vulnerable period before allowing the shield to respawn
            StartCoroutine(ShieldVulnerabilityPhase());
        }
    }

    IEnumerator ShieldVulnerabilityPhase()
    {
        // Wait for a short time where the panther is vulnerable
        yield return new WaitForSeconds(vulnerabilityTime);

        canActivateShield = true;
        // Allow shield to be reactivated after a delay
        StartCoroutine(RespawnShieldDelayed());
    }



    void BreakShield()
    {
        if (!shieldActive) return;

        shieldActive = false;

        foreach (GameObject shield in activeShields)
        {
            if (shield != null)
            {
                Rigidbody2D rb = shield.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 fireDirection = (shield.transform.position - transform.position).normalized;
                    rb.velocity = fireDirection * projectileSpeed;
                }
                Destroy(shield, 2f);
            }
        }

        activeShields.Clear();

        // Instead of immediately respawning, enter vulnerability phase first
        StartCoroutine(ShieldVulnerabilityPhase());
    }


    IEnumerator RespawnShieldDelayed()
    {
        // Wait for the respawn delay time
        yield return new WaitForSeconds(respawnShieldDelay);

        canActivateShield = true;
    }


    void FacePlayer()
    {
        Vector2 playerDirection = (player.position - transform.position).normalized;
        animator.SetFloat("posX", playerDirection.y);
        animator.SetFloat("posY", playerDirection.x);
    }

    public void Damage(int damage)
    {
        if (currentState == EnemyState.Dead) { return; }

        BossEnemy enemy = GetComponentInParent<BossEnemy>();
        enemy.Damage(damage);
    }

    public bool IsShieldActive()
    {
        return shieldActive;
    }
}
