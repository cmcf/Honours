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
    public Transform player;
    public Transform[] boundaryPoints;
    private Animator animator;
    private Rigidbody2D rb;
    public Room currentRoom;
    TrailRenderer trailRenderer;

    // Movement Variables
    public float chargeSpeed = 8f;
    public float chargeCooldown = 5f;
    public float chargeDuration = 1f;
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float attackSpeed = 2f;
    float chargeStartTime;
    private bool isCharging = false;

    // Melee Attack Variables
    public float attackRange = 2f;
    public float stopAttackRange = 4f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    bool canAttack = true;

    bool isAttacking = false;

    // Shield Variables
    public GameObject shieldPrefab;
    public Transform shieldSpawnPoint;
    private List<GameObject> activeShields = new List<GameObject>();
    public bool shieldActive = false;
    public float shieldDuration = 8f; // Time before the shield projectiles fire
    [SerializeField] float respawnShieldDelay = 2f;
    [SerializeField] float defendStateDuration = 25f;

    public Transform target;
    public float orbitRadius = 0.5f;
    public float orbitSpeed = 100f; // Degrees per second
    public float projectileSpeed = 5f; // Speed of the fired shield projectiles

    float angle; // Tracks rotation angle

    Vector2 chargeDirection; // Stores the direction during the charge

    bool canAttackAgain = true;
    [SerializeField] bool canActivateShield = true;
    float defendStateStartTime;

    float shieldCooldown = 1f;
    float lastShieldTime;
    public float shieldCount = 5;
    public EnemyState currentState;
    public enum PantherState { Defend, Charge, Attack, Follow }
    public PantherState currentPhase = PantherState.Defend;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void FixedUpdate()
    {
        if (currentPhase == PantherState.Follow)
        {
            FollowPlayer();
        }

    }

    void OnEnable()
    {
        canActivateShield = true;
        defendStateStartTime = Time.time;
        currentPhase = PantherState.Defend;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) { return; }

        player = GameObject.FindGameObjectWithTag("Player").transform;

        // If the shield is active, stop charging and movement
        if (shieldActive)
        {
            animator.SetFloat("speed", 0);
            rb.velocity = Vector2.zero;
            UpdateShieldOrbit();
            return; 
        }

        // Handle Charge Phase
        if (currentPhase == PantherState.Charge)
        {
            StartChargePhase();
        }

        if (currentPhase == PantherState.Defend)
        {
            FacePlayer();

            if (canActivateShield)
            {
                ActivateShield();
            }

            // Transition to Charge after the defend duration ends
            if (Time.time - defendStateStartTime >= defendStateDuration)
            {
                currentPhase = PantherState.Charge;
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
        if (Time.time >= chargeCooldown)
        {
            isCharging = true;
            animator.SetFloat("speed", chargeSpeed);  // Set speed for animation
            chargeDirection = (player.position - transform.position).normalized;
            animator.SetFloat("posX", chargeDirection.y);
            animator.SetFloat("posY", chargeDirection.x);
            trailRenderer.emitting = true;

            // Record the time when the charge starts
            chargeStartTime = Time.time;
            PerformCharge();
        }
    }

    void PerformCharge()
    {
        // Check if the Panther is near a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDirection, 1f);

        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            // If the Panther is blocked by a wall, stop moving
            rb.velocity = Vector2.zero;
        }

        rb.velocity = chargeDirection * chargeSpeed;

        // Check if the charge duration has passed
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isAttacking", true);
            AttackPlayer();
        }

        if (Time.time - chargeStartTime >= chargeDuration)
        {
            rb.velocity = Vector2.zero;
            currentPhase = PantherState.Defend;
        }
    }

    void FollowPlayer()
    {
        if (currentPhase != PantherState.Follow)
        {
            return; // Don't follow if not in Follow state
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 moveDirection = (player.position - transform.position).normalized;

        // Check if the Panther is near a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 1f);

        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            // If the Panther is blocked by a wall, stop moving
            rb.velocity = Vector2.zero;
            return; // Stop moving towards the player
        }

        // Prevent jittering by not re-entering attack state constantly
        if (distanceToPlayer <= attackRange)
        {
            if (!isAttacking && canAttackAgain)
            {
                currentPhase = PantherState.Attack;
                AttackPlayer();
            }
            rb.velocity = Vector2.zero; // Stop moving when attacking
        }
        else
        {
            // Move towards the player if no wall is blocking
            rb.velocity = moveDirection * moveSpeed;
        }

        // If the Panther is too far, go back to Defend state
        if (distanceToPlayer > stopAttackRange)
        {
            currentPhase = PantherState.Defend;
            rb.velocity = Vector2.zero;
            ActivateShield();
        }

        // Update animation direction
        animator.SetFloat("posX", moveDirection.y);
        animator.SetFloat("posY", moveDirection.x);
        animator.SetBool("speed", rb.velocity != Vector2.zero);
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

        // After attack, continue following the player
        currentPhase = PantherState.Follow;

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
        yield return new WaitForSeconds(1f);
        currentPhase = PantherState.Defend;
    }

    public void ActivateShield()
    {
        if (shieldActive) return;

        canActivateShield = false;

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
        float vulnerabilityTime = 3f; 

        // Wait for a short time where the panther is vulnerable
        yield return new WaitForSeconds(vulnerabilityTime);

        canActivateShield = true;
        // Now allow shield to be reactivated
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

    public void Damage(float damage)
    {
        if (currentState == EnemyState.Dead) { return; }

        BossEnemy enemy = GetComponentInParent<BossEnemy>();
        enemy.Damage(damage);
    }

    public bool IsShieldActive()
    {
        return shieldActive;
    }

    public void Damage(int damage)
    {
        throw new NotImplementedException();
    }
}
