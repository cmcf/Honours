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
    [SerializeField] int knifeCount = 6; 
    [SerializeField] float shieldDuration = 3f; // Time before firing knives
  
    public float knifeCooldown = 0.5f;
    float lastKnifeTime;

    [Header("Bite")]
    [SerializeField] Transform bitePoint;  // Point where the bite attack is focused
    [SerializeField] float biteRange = 0.5f;  // Radius of the bite hitbox
    [SerializeField] int minBiteDamage = 15;
    [SerializeField] int maxBiteDamage = 20;
    int biteDamage;  // Damage dealt by the bite attack

    [SerializeField] LayerMask enemyLayer;  // Layer of enemies
    [SerializeField] LayerMask shellLayer;
    LayerMask combinedLayerMask;

    [Header("Movement")]
    [SerializeField] float lungeDistance = 0.5f;

    float biteCooldown = 0.2f;
    float lastBiteTime;

    public Vector2 moveDirection;
    Vector2 lastMoveDirection; // Tracks last movement direction
    public bool isBiting = false; // Track if the player is biting
    public bool canMoveWolf;

    bool knivesActive = false;
    bool canBite = true;
    bool isDead = false;

    public float Health { get; set; }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentSpeed = defaultSpeed;
        biteDamage = Random.Range(minBiteDamage, maxBiteDamage + 1);
        playerHealth = FindObjectOfType<PlayerHealth>();

        // Combine the Enemy and Shell layers into one mask
        combinedLayerMask = enemyLayer | shellLayer;
    }

    void Update()
    {
        UpdateAnimation();

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
        if (knivesActive) return; 

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
        if (Time.time > lastBiteTime + biteCooldown && !isBiting)
        {
            lastBiteTime = Time.time;

            // Find closest enemy within detection range
            Collider2D[] enemies = Physics2D.OverlapCircleAll(bitePoint.position, biteRange * 2f, combinedLayerMask);
            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            // Always bite, but only lunge if enemy is far
            if (closestEnemy == null || closestDistance > lungeDistance)
            {
                StartCoroutine(BiteLunge());
            }

            SetBiteDirection();
            StartCoroutine(EndBiteAttackCoroutine());
            animator.SetBool("isBiting", true);
            isBiting = true;
        }
    }



    IEnumerator BiteLunge()
    {
        float lungeDuration = 0.1f; // Duration of the lunge
        float elapsedTime = 0f;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + lastMoveDirection.normalized * lungeDistance; 

        while (elapsedTime < lungeDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsedTime / lungeDuration));
            elapsedTime += Time.deltaTime;

            isBiting = true;
            animator.SetBool("isBiting", true);
            currentSpeed = attackMoveSpeed;

            yield return null;
        }

        rb.MovePosition(targetPos); // Ensure final position is set correctly
    }


    IEnumerator EndBiteAttackCoroutine()
    {
        yield return new WaitForSeconds(0.2f); 
        isBiting = false;
        currentSpeed = defaultSpeed;
        animator.SetBool("isBiting", false);

        // Resume movement if the player is still pressing movement keys
        if (moveDirection != Vector2.zero)
        {
            rb.velocity = moveDirection * currentSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        UpdateAnimation();
    }
    public void EndBiteAttack()
    {
        isBiting = false;
        currentSpeed = defaultSpeed;
        animator.SetBool("isBiting", false);

        // If the player is still holding a movement direction, resume movement
        if (moveDirection != Vector2.zero)
        {
            rb.velocity = moveDirection * currentSpeed;
        }
        else
        {
            // Ensure the rigidbody isn't stuck
            rb.velocity = Vector2.zero;
        }

        UpdateAnimation();
    }


    public void BiteDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(bitePoint.position, biteRange, combinedLayerMask);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // Apply the modifier damage
                damageable.Damage(biteDamage);

                // Apply the modifier effect
                biteModifier.ApplyEffect(damageable, this);

                if (isBiting)
                {
                    StopCoroutine(EndBiteAttackCoroutine());
                    EndBiteAttack();
                }
            }

            // Check if the bite hits the shield 
            if (enemy.CompareTag("Shield")) 
            {
                DestroyShield(enemy.gameObject);  // Destroy the shield
            }
        }
    }
    public void DestroyShield(GameObject shield)
    {
        
        Panther panther = FindObjectOfType<Panther>(); 

        if (panther != null)
        {
            panther.OnShieldDestroyed(shield); // Notify Panther that the shield is destroyed
        }

        Destroy(shield); // Destroy the shield GameObject
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

    void OnDrawGizmosSelected()
    {
        if (bitePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bitePoint.position, biteRange);
        }
    }

    public void Damage(int damage)
    {
        if (isDead) { return; }
        // Current health is decreased by the damage received
        playerHealth.TakeDamage(damage);
    }

    public void DestroyKnives()
    {
        // Loop through all the orbiting knives and destroy them
        foreach (GameObject knife in orbitingKnives)
        {
            if (knife != null)
            {
                Destroy(knife); 
            }
        }

        // Clear the list after destroying the knives
        orbitingKnives.Clear();
        knivesActive = false;  
    }



}
