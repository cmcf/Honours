﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class Panther : MonoBehaviour, IDamageable
{
    public Transform player;
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
    private bool isCharging = false;

    // Melee Attack Variables
    public float attackRange = 2f;
    public float stopAttackRange = 4f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private bool canAttack = true;

    bool isAttacking = false;

    // Shield Variables
    public GameObject shieldPrefab;
    public Transform shieldSpawnPoint;
    private List<GameObject> activeShields = new List<GameObject>();
    private bool shieldActive = false;
    public float shieldDuration = 8f; // Time before the shield projectiel fire

    public Transform target; 
    public float orbitRadius = 0.5f;
    public float orbitSpeed = 100f; // Degrees per second
    public float projectileSpeed = 5f; // Speed of the fired shield projectiles

    float angle; // Tracks rotation angle

    Vector2 chargeDirection; // Stores the direction during the charge

    float shieldCooldown = 1f;
    float lastShieldTime;
    public float shieldCount = 5;
    public EnemyState currentState;
    public enum PantherState { Defend, Charge, Attack }
    public PantherState currentPhase = PantherState.Defend;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();

        StartCoroutine(ShieldCycle());
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) { return; }
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Only update direction when not charging
        if (!isCharging)
        {
            Vector2 moveDirection = rb.velocity.normalized;
            if (moveDirection.magnitude > 0.1f)
            {
                animator.SetFloat("posX", moveDirection.y);
                animator.SetFloat("posY", moveDirection.x);

            }
        }

        // Face player only when idle
        if (currentPhase == PantherState.Defend)
        {
            FacePlayer();
        }

        // Handle speed animation update
        animator.SetFloat("speed", rb.velocity.magnitude);

        ClampPosition();

        // Check for the current phase and activate the shield in the Defend phase
        if (currentPhase == PantherState.Defend)
        {
            ActivateShield();
            UpdateShieldOrbit();
        }
    }

    public void StartAttacking()
    {
        if (!isCharging)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    IEnumerator ChargeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(chargeCooldown);

            if (currentState != EnemyState.Dead && player != null)
            {
                StartCoroutine(ChargeAtPlayer());
            }
        }
    }

    IEnumerator ShieldCycle()
    {
        while (true)
        {
            if (currentPhase == PantherState.Defend)
            {
                ActivateShield();
                yield return new WaitForSeconds(shieldDuration);
                BreakShield();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void ActivateShield()
    {
        // Prevent multiple activations
        if (shieldActive) return; 

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
                // Parent to panther
                GameObject shield = Instantiate(shieldPrefab, shieldPos, Quaternion.identity, transform); 
                activeShields.Add(shield);
            }

            shieldActive = true;

            // Break shield after duration
            Invoke(nameof(BreakShield), shieldDuration);
        }
    }

    // Keeps shields orbiting around the Panther
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

            // Rotate shields to always face outward
            float rotationAngle = Mathf.Atan2(shieldPos.y - transform.position.y, shieldPos.x - transform.position.x) * Mathf.Rad2Deg;
            activeShields[i].transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }


    public void BreakShield()
    {
        if (!shieldActive) return;

        Debug.Log("Panther shield is broken!");
        shieldActive = false;

        foreach (GameObject shield in activeShields)
        {
            if (shield != null)
            {
                Rigidbody2D rb = shield.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Calculate direction based on current position relative to the panther
                    Vector2 fireDirection = (shield.transform.position - transform.position).normalized;
                    rb.velocity = fireDirection * projectileSpeed; // Fire outward in its orbiting direction
                }

                Destroy(shield, 2f);
            }
        }

        activeShields.Clear();
        StartCoroutine(ChargeAtPlayer()); // Continue to next phase
    }

    IEnumerator ChargeAtPlayer()
    {
        currentPhase = PantherState.Charge;
        isCharging = true;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        animator.SetFloat("speed", chargeSpeed);

        // Store the initial charge direction and move toward the player
        chargeDirection = (player.position - transform.position).normalized;
        animator.SetFloat("posX", chargeDirection.y);
        animator.SetFloat("posY", chargeDirection.x);

        // Dash towards the player
        rb.velocity = chargeDirection * chargeSpeed;

        float chargeTimeElapsed = 0f;

        while (chargeTimeElapsed < chargeDuration)
        {
            chargeTimeElapsed += Time.deltaTime;

            // If the panther is within attack range, stop movement and attack
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                rb.velocity = Vector2.zero; // Stop movement
                animator.SetBool("isAttacking", true);
                AttackPlayer();
                yield break;
            }

            // Keep moving towards the player until the charge duration ends
            rb.velocity = chargeDirection * chargeSpeed;
            animator.SetFloat("speed", chargeSpeed);

            yield return null;
        }

        // End charge and stop
        rb.velocity = Vector2.zero;
        isCharging = false;

        // Transition to Defend phase if the attack was not triggered
        currentPhase = PantherState.Defend;
    }


    IEnumerator FollowPlayer()
    {
        isCharging = false;
        Debug.Log("Panther is following the player!");

        // Dash towards the player's new position
        Vector2 moveDirection = (player.position - transform.position).normalized;
        rb.velocity = moveDirection * chargeSpeed;

        float followDuration = 0.5f; // Short dash duration
        float elapsed = 0f;

        while (elapsed < followDuration)
        {
            elapsed += Time.deltaTime;

            // If within attack range, stop movement and attack
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                rb.velocity = Vector2.zero; // Stop movement
                animator.SetBool("isAttacking", true);
                AttackPlayer();
                yield break;
            }

            yield return null;
        }

        // After following the player, stop movement and check next action
        rb.velocity = Vector2.zero;

        // If the player moved too far, go back to defend
        if (Vector2.Distance(transform.position, player.position) > attackRange)
        {
            currentPhase = PantherState.Defend;
        }
        else
        {
            animator.SetBool("isAttacking", false);
            Debug.Log("Panther missed the player. Returning to Defend state.");
        }
    }


    void AttackPlayer()
    {
        if (!isAttacking)
        {
            currentPhase = PantherState.Attack;
            isAttacking = true;
            animator.SetBool("isAttacking", true);

            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        float attackElapsed = 0f;

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

        // Only deal damage if the player is still within range
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            AttackDamage();
        }

        StopAttack();
    }

    void StopAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        rb.velocity = Vector2.zero; // Stop any movement during attack
        Debug.Log("Panther stopped attacking. Returning to Defend phase.");

        // After attack, wait briefly before returning to Defend state
        StartCoroutine(ReturnToDefendState());
    }



    IEnumerator ReturnToDefendState()
    {
        yield return new WaitForSeconds(1f); // Small pause before resetting
        currentPhase = PantherState.Defend;
        Debug.Log("Panther has returned to Defend phase.");
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

    void ClampPosition()
    {
        float margin = 2f;

        // Calculate the room's boundaries with margin
        float roomMinX = currentRoom.GetRoomCentre().x - (currentRoom.width / 2) + margin;
        float roomMaxX = currentRoom.GetRoomCentre().x + (currentRoom.width / 2) - margin;
        float roomMinY = currentRoom.GetRoomCentre().y - (currentRoom.height / 2) + 1.12f;
        float roomMaxY = currentRoom.GetRoomCentre().y + (currentRoom.height / 2) - margin;

        // Get current position
        Vector3 currentPosition = transform.position;
        float clampedX = Mathf.Clamp(currentPosition.x, roomMinX, roomMaxX);
        float clampedY = Mathf.Clamp(currentPosition.y, roomMinY, roomMaxY);

        // Determine the direction of the Panther's movement relative to the clamped position
        Vector2 velocity = rb.velocity;

        // If the Panther is near the edges, decelerate smoothly
        if (currentPosition.x != clampedX || currentPosition.y != clampedY)
        {
            // If the Panther is at or near the boundary, smooth out the deceleration
            velocity = Vector2.Lerp(velocity, Vector2.zero, 0.5f);

            // Set the velocity
            rb.velocity = velocity;

            // Stop any angular velocity to prevent unwanted rotation
            rb.angularVelocity = 0f;

            // Stop the animation
            animator.SetFloat("speed", 0f);
        }

        // Apply the clamped position if the Panther is out of bounds
        transform.position = new Vector3(clampedX, clampedY, currentPosition.z);

    }


    public void AttackDamage()
    {
        // Check if the player has an IDamageable component and apply damage
        if (player.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            // Call the player's Damage method with the attack damage
            damageable.Damage(attackDamage);
        }
    }
}
