using System.Collections;
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
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private bool canAttack = true;

    public EnemyState currentState;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer= GetComponent<TrailRenderer>();
        // Start charge behaviour loop
        StartCoroutine(ChargeRoutine());
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) { return; }

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

        // Handle speed animation update
        animator.SetFloat("speed", rb.velocity.magnitude);

        ClampPosition();
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

    private Vector2 chargeDirection; // Store the direction during the charge

    IEnumerator ChargeAtPlayer()
    {
        isCharging = true;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        animator.SetFloat("speed", chargeSpeed);

        // Store charge direction
        chargeDirection = (player.position - transform.position).normalized;

        // Set animation direction
        animator.SetFloat("posX", chargeDirection.y);
        animator.SetFloat("posY", chargeDirection.x);

        // Move in a straight line
        rb.velocity = chargeDirection * chargeSpeed;

        float chargeTimeElapsed = 0f;
        while (chargeTimeElapsed < chargeDuration)
        {
            chargeTimeElapsed += Time.deltaTime;

            // Check if close enough to attack
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                moveSpeed = attackSpeed;
                StartCoroutine(AttackPlayer());
                yield break;
            }

            yield return null;
        }

        // Stop movement after charge
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        animator.SetFloat("speed", 0f);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        isCharging = false;

        // Face the player
        Vector2 playerDirection = (player.position - transform.position).normalized;
        animator.SetFloat("posX", playerDirection.y);
        animator.SetFloat("posY", playerDirection.x);
    }

    IEnumerator AttackPlayer()
    {
        isCharging = false;
        canAttack = false;

        // Use the charge direction for the attack animation
        animator.SetFloat("posX", chargeDirection.y);
        animator.SetFloat("posY", chargeDirection.x);

        // Start the attack animation
        animator.SetBool("isAttacking", true);

        // Store the player's initial position
        Vector2 playerInitialPosition = player.position;

        // Let the attack animation play through its full duration
        float attackTimeElapsed = 0f;

        while (attackTimeElapsed < attackCooldown)
        {
            attackTimeElapsed += Time.deltaTime;

            yield return null;
        }

        // End the attack animation after the cooldown time has passed
        animator.SetBool("isAttacking", false);

        // Allow the panther to attack again after the cooldown
        canAttack = true;
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

}
