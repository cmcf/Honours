using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using static Damage;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class Panther : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Transform[] boundaryPoints;
    private Animator animator;
    private Rigidbody2D rb;
    public Room currentRoom;
    public Transform target;
    TrailRenderer trailRenderer;

    [Header("Movement")]
    public float chargeSpeed = 8f;
    public float chargeCooldown = 5f;
    public float chargeDuration = 1f;
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float attackSpeed = 2f;
    float chargeStartTime;
    bool isCharging = false;

    [Header("Melee")]
    public float attackRange = 2f;
    public float stopAttackRange = 4f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;

    bool canAttack = true;
    bool isAttacking = false;
    bool canAttackAgain = true;

    [Header("Shield")]
    public GameObject shieldPrefab;
    public Transform shieldSpawnPoint;
    List<GameObject> activeShields = new List<GameObject>();
    public bool shieldActive = false;
    public float shieldDuration = 8f; // Time before the shield projectiles fire
    public float orbitRadius = 0.5f;
    public float orbitSpeed = 100f; // Degrees per second
    public float projectileSpeed = 5f; // Speed of the fired shield projectiles
    [SerializeField] float respawnShieldDelay = 2f;
    [SerializeField] float defendStateDuration = 25f;
    [SerializeField] float vulnerabilityTime = 8f;
    [SerializeField] float shieldCooldown = 2f;
    [SerializeField] bool canActivateShield = true;

    public float shieldCount = 5;

    bool shieldScheduledThisPhase = false;
    float lastShieldTime;
    float angle; // Tracks rotation angle
    float defendStateStartTime;

    Vector2 chargeDirection; // Stores the direction during the charge
     
    public EnemyState currentState;
    public enum PantherState { Defend, Charge, Attack }
    public PantherState currentPhase = PantherState.Defend;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
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
        isCharging = false;
        trailRenderer.emitting = false;
        currentPhase = PantherState.Defend;
        defendStateStartTime = Time.time; // Reset defend timer
        shieldScheduledThisPhase = false; // Allow shield again
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

    IEnumerator ReturnToDefendState()
    {
        yield return new WaitForSeconds(3f);
        currentPhase = PantherState.Defend;
        shieldScheduledThisPhase = false; // Reset on state return
    }

    public void ActivateShield()
    {
        if (shieldActive) return;

        canActivateShield = false;
        shieldScheduledThisPhase = false;

        if (Time.time > lastShieldTime + shieldCooldown)
        {
            lastShieldTime = Time.time;
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

            // Break shield after duration
            Invoke(nameof(BreakShield), shieldDuration);
        }
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


    void UpdateShieldOrbit()
    {
        if (!shieldActive) return;

        for (int i = 0; i < activeShields.Count; i++)
        {
            if (activeShields[i] == null) continue;

            float angle = (Time.time * orbitSpeed + (360f / activeShields.Count) * i) * Mathf.Deg2Rad;

            Vector3 shieldPos = new Vector3(
                transform.position.x + Mathf.Cos(angle) * orbitRadius,
                transform.position.y + Mathf.Sin(angle) * orbitRadius,
                0f
            );

            activeShields[i].transform.position = shieldPos;
        }
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
