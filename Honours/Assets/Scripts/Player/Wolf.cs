using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Wolf : MonoBehaviour, IDamageable
{
    Rigidbody2D rb;
    Animator animator;
    [SerializeField] float defaultSpeed = 6f;
    [SerializeField] float currentSpeed = 6f;
    [SerializeField] float attackMoveSpeed = 2f;
    public BiteModifier biteModifier;
    PlayerHealth playerHealth;

    [SerializeField] Transform bitePoint;  // Point where the bite attack is focused
    [SerializeField] float biteRange = 0.5f;  // Radius of the bite hitbox
    [SerializeField] int biteDamage = 10;  // Damage dealt by the bite attack
    [SerializeField] LayerMask enemyLayer;  // Layer of enemies

    public Vector2 moveDirection;
    private Vector2 lastMoveDirection; // Tracks last movement direction
    public bool isBiting = false; // Track if the player is biting
    public bool canMoveWolf;

    bool isDead = false;

    public float Health { get; set; }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentSpeed = defaultSpeed;
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        // Only update movement animations if not biting
        if (!isBiting) 
        {
            UpdateAnimation();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
    }

    public void DisableInput()
    {
        moveDirection = Vector2.zero;
    }

    void Move()
    {
        rb.velocity = moveDirection * currentSpeed;
    }

    public void EquipBiteEffect(BiteModifier newBiteEffect)
    {
        biteModifier = newBiteEffect;
    }


    public void BiteAttack()
    {
        if (!isBiting) // Check if not already biting
        {
            // Play bite animation
            isBiting = true; 
            animator.SetBool("isBiting", true); 
            currentSpeed = attackMoveSpeed;
            SetBiteDirection(); // Set the correct bite direction based on movement
        }

        Invoke("EndBiteAttack", 0.3f); 
    }


    private void SetBiteDirection()
    {
        int direction = 0; // Default to Down

        // Set direction based on last move direction
        if (lastMoveDirection.y > 0) direction = 1;  // Up
        else if (lastMoveDirection.y < 0) direction = 0;  // Down
        else if (lastMoveDirection.x < 0) direction = 2;  // Left
        else if (lastMoveDirection.x > 0) direction = 3;  // Right

        animator.SetInteger("BiteDirection", direction); // Set the BiteDirection parameter
    }

    void UpdateAnimation()
    {
        float speed = moveDirection.magnitude;
        Vector2 normalizedDirection = Vector2.zero;

        if (speed > 0)
        {
            normalizedDirection = moveDirection.normalized;
            lastMoveDirection = normalizedDirection;
        }

        // Update movement animations with last move direction
        animator.SetFloat("animMoveX", lastMoveDirection.x); // Set X direction
        animator.SetFloat("animMoveY", lastMoveDirection.y); // Set Y direction
        animator.SetFloat("speed", speed); // Set speed for idle/walk transitions
    }

    public void BiteDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(bitePoint.position, biteRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // Apply the modifier damage
                damageable.Damage(biteDamage); 

                // Apply the modifier effect
                biteModifier.ApplyEffect(damageable, this); 
            }
        }
    }

    public void EndBiteAttack()
    {
        // Reset isBiting flag to allow movement animation states
        isBiting = false;
        currentSpeed = defaultSpeed;
        animator.SetBool("isBiting", false);
        UpdateAnimation();
    }

    void OnDrawGizmosSelected()
    {
        if (bitePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bitePoint.position, biteRange);
        }
    }

    public void Damage(float damage)
    {
        if (isDead) { return; }
        // Current health is decreased by the damage received
        playerHealth.TakeDamage(damage);
    }

}
