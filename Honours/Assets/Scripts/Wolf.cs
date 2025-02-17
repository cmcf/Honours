using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Wolf : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    [SerializeField] float moveSpeed = 6f;
    public bool canMoveWolf;

    [SerializeField] Transform bitePoint;  // Point where the bite attack is focused
    [SerializeField] float biteRange = 0.5f;  // Radius of the bite hitbox
    [SerializeField] int biteDamage = 10;  // Damage dealt by the bite attack
    [SerializeField] LayerMask enemyLayer;  // Layer of enemies

    public Vector2 moveDirection;
    Vector2 lastMoveDirection; // Tracks last movement direction
    public bool isBiting; // Track if the player is biting

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isBiting) // Only update movement animations if not biting
        {
            UpdateAnimation();
        }
    }

    void FixedUpdate()
    {
        if (!isBiting) // Prevent movement during bite attack
        {
            Move();
        }
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
        rb.velocity = moveDirection * moveSpeed;
    }

    public void BiteAttack()
    {
        if (!isBiting) // Check if not already biting
        {
            isBiting = true; // Set isBiting to true
            animator.SetBool("isBiting", true); // Set bool to true to play the bite animation
            int direction = GetFacingDirection(); // Get the correct facing direction
            animator.SetInteger("Direction", direction); // Set the direction parameter for the animation
        }
    }

    private int GetFacingDirection()
    {
        if (lastMoveDirection.y > 0) return 1;  // Up
        if (lastMoveDirection.y < 0) return 0;  // Down
        if (lastMoveDirection.x < 0) return 2;  // Left
        return 3;  // Right
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

        // Update movement animations
        animator.SetFloat("animMoveX", lastMoveDirection.x);
        animator.SetFloat("animMoveY", lastMoveDirection.y);
        animator.SetFloat("speed", speed);
    }

    // Called by the animation event at the correct moment
    public void BiteDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(bitePoint.position, biteRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Damage(biteDamage);
            }
        }
    }

    // Called at the **end** of the bite animation (
    public void EndBiteAttack()
    {
        isBiting = false; // Reset isBiting flag to allow movement again
        animator.SetBool("isBiting", false); // Set the isBiting bool back to false to transition out of bite animation
    }

    void OnDrawGizmosSelected()
    {
        if (bitePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bitePoint.position, biteRange);
        }
    }
}
