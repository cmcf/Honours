using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Wolf : MonoBehaviour, IDamageable
{
    [Header("References")]
    Rigidbody2D rb;
    Animator animator;
    PlayerHealth playerHealth;
    public BiteModifier biteModifier;
    public Transform firePoint;
    public GameObject knifePrefab;
   
    [Header("Speed")]
    [SerializeField] float defaultSpeed = 6f;
    [SerializeField] float currentSpeed = 6f;
    [SerializeField] float attackMoveSpeed = 2f;

    [Header("Knife")]
    List<GameObject> orbitingKnives = new List<GameObject>();
    [SerializeField] float knifeSpeed = 7f;
    [SerializeField] float orbitRadius = 0.5f; // Distance from player
    [SerializeField] float orbitSpeed = 200f;  // Rotation speed
    [SerializeField] int knifeCount = 8; 
    [SerializeField] float shieldDuration = 3f; // Time before firing knives
  
    public float knifeCooldown = 0.5f;
    float lastKnifeTime;

    [Header("Bite")]
    [SerializeField] Transform bitePoint;  // Point where the bite attack is focused
    [SerializeField] float biteRange = 0.5f;  // Radius of the bite hitbox
    [SerializeField] int biteDamage = 10;  // Damage dealt by the bite attack
    [SerializeField] LayerMask enemyLayer;  // Layer of enemies

    public Vector2 moveDirection;
    private Vector2 lastMoveDirection; // Tracks last movement direction
    public bool isBiting = false; // Track if the player is biting
    public bool canMoveWolf;

    bool knivesActive = false;
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

        if (knivesActive)
        {
            UpdateKnifeOrbit();
        }

    }

    void OnDash()
    {
        if (isDead) { return; }

        ActivateKnifeShield();
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

    // Spawns knives in an orbiting pattern and starts timer to fire
    public void ActivateKnifeShield()
    {
        if (knivesActive) return; // Prevent multiple activations

        if (Time.time > lastKnifeTime + knifeCooldown)
        {
            lastKnifeTime = Time.time;
            float angleStep = 360f / knifeCount;

            for (int i = 0; i < knifeCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;

                Vector3 knifePos = new Vector3(
                    transform.position.x + Mathf.Cos(angle) * orbitRadius,
                    transform.position.y + Mathf.Sin(angle) * orbitRadius,
                    0f
                );

                GameObject knife = Instantiate(knifePrefab, knifePos, Quaternion.identity);
                orbitingKnives.Add(knife);
            }

            knivesActive = true;

            // Fire knives after delay
            Invoke(nameof(FireKnives), shieldDuration);
        }

        
    }

    // Keeps knives orbiting around the wolf
    void UpdateKnifeOrbit()
    {
        // Stop updating if knives should fire
        if (!knivesActive) return; 

        for (int i = 0; i < orbitingKnives.Count; i++)
        {
            if (orbitingKnives[i] == null) continue;

            float angle = (Time.time * orbitSpeed + (360f / orbitingKnives.Count) * i) * Mathf.Deg2Rad;

            Vector3 knifePos = new Vector3(
                transform.position.x + Mathf.Cos(angle) * orbitRadius,
                transform.position.y + Mathf.Sin(angle) * orbitRadius,
                0f
            );

            orbitingKnives[i].transform.position = knifePos;

            // Rotate knives to always face outward
            float rotationAngle = Mathf.Atan2(knifePos.y - transform.position.y, knifePos.x - transform.position.x) * Mathf.Rad2Deg;
            orbitingKnives[i].transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }

    void FireKnives()
    {
        // Stop orbiting
        knivesActive = false; 

        foreach (GameObject knife in orbitingKnives)
        {
            if (knife == null) continue;

            // Get the Knife script and call Fire function
            KnifeProjectile knifeScript = knife.GetComponent<KnifeProjectile>();
            if (knifeScript != null)
            {
                Vector3 direction = (knife.transform.position - transform.position).normalized;
                // Fire outward set knife speed
                knifeScript.Fire(direction, knifeSpeed); 
            }
        }
        // Clear the list after firing

        orbitingKnives.Clear(); 
    }


    public void BiteAttack()
    {
        if (!isBiting) // Check if not already biting
        {
            // Play bite animation
            isBiting = true; 
            animator.SetBool("isBiting", true); 
            currentSpeed = attackMoveSpeed;
            // Set the correct bite direction based on movement
            SetBiteDirection();
        }

        Invoke("EndBiteAttack", 0.3f); 
    }


    void SetBiteDirection()
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
