﻿using System.Collections;
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
        // Start charge behaviour loop
        StartCoroutine(ChargeRoutine());
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

    void ActivateShield()
    {
        Debug.Log("Panther shield is active");

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

        // Store initial charge direction
        chargeDirection = (player.position - transform.position).normalized;

        // Update the animator with initial direction
        animator.SetFloat("posX", chargeDirection.y);
        animator.SetFloat("posY", chargeDirection.x);

        // Move in a straight line towards the player
        rb.velocity = chargeDirection * chargeSpeed;

        float chargeTimeElapsed = 0f;
        bool hasAttacked = false;

        while (chargeTimeElapsed < chargeDuration)
        {
            chargeTimeElapsed += Time.deltaTime;

            // Continuously update the charge direction based on the player's current position
            chargeDirection = (player.position - transform.position).normalized;

            // Update animator with the new charge direction
            animator.SetFloat("posX", chargeDirection.y);
            animator.SetFloat("posY", chargeDirection.x);

            // Check if the Panther is within attack range
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                // Only attack once when entering the range
                if (!hasAttacked)
                {
                    hasAttacked = true;
                    AttackPlayer();  
                }

                // Stop movement once in range
                rb.velocity = Vector2.zero;

                // Continue attacking while still in range
                if (!isAttacking)
                {
                    AttackPlayer();
                }
            }
            else
            {
                // Continue charging if player is out of range
                rb.velocity = chargeDirection * chargeSpeed;
            }

            yield return null;
        }

        // Movement stop and reset after charge
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        animator.SetFloat("speed", 0f);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        isCharging = false;

        // Wait for the attack cooldown to finish before switching to Defend phase
        yield return new WaitForSeconds(attackCooldown);
        currentPhase = PantherState.Defend;
    }


    void AttackPlayer()
    {
        if (!isAttacking)
        {
            currentPhase = PantherState.Attack;
            isAttacking = true;
            animator.SetBool("isAttacking", true);

            AttackDamage();

            // Schedule attack end after attack cooldown
            Invoke(nameof(StopAttack), attackCooldown);
        }
    }

    void StopAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        currentPhase = PantherState.Defend;
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
        float roomMinY = currentRoom.GetRoomCentre().y - (currentRoom.height / 2) + 1.3f;
        float roomMaxY = currentRoom.GetRoomCentre().y + (currentRoom.height / 2) - margin;

        // Get current position
        Vector3 currentPosition = transform.position;
        float clampedX = Mathf.Clamp(currentPosition.x, roomMinX, roomMaxX);
        float clampedY = Mathf.Clamp(currentPosition.y, roomMinY, roomMaxY);

        // If clamping happened, stop movement
        if (currentPosition.x != clampedX || currentPosition.y != clampedY)
        {
            rb.velocity = Vector2.zero;  // Stop movement
            rb.angularVelocity = 0f; // Stop rotation
        }

        // Apply the clamped position
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
