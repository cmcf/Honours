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
        // Constantly gets player current location
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // If charging, update direction and move
        if (isCharging)
        {
            // Enable the trail when charging
            if (trailRenderer != null && !trailRenderer.enabled)
            {
                trailRenderer.enabled = true;
            }

            // Update direction based on charge towards the player
            Vector2 chargeDirection = (player.position - transform.position).normalized;
            animator.SetFloat("posX", chargeDirection.y); // Update posX
            animator.SetFloat("posY", chargeDirection.x); // Update posY

            ClampPosition();
            return;  
        }

        // Disable the trail when not charging
        if (trailRenderer != null && trailRenderer.enabled)
        {
            trailRenderer.emitting = false;
        }

        // Check if the panther is moving or stopped to update the animation
        if (rb.velocity.magnitude > 0.1f)
        {
            animator.SetFloat("speed", rb.velocity.magnitude);  // Smoothly update speed for moving animation
        }
        else
        {
            animator.SetFloat("speed", 0f); 
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

    IEnumerator ChargeAtPlayer()
    {
        isCharging = true;

        // Enable the trail during the charge
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        animator.SetFloat("speed", chargeSpeed); 

        // Dash through the player while charging
        Vector2 chargeDirection = (player.position - transform.position).normalized;
        rb.velocity = chargeDirection * chargeSpeed;

        // Keep charging for the duration of the charge
        float chargeTimeElapsed = 0f;
        while (chargeTimeElapsed < chargeDuration)
        {
            chargeTimeElapsed += Time.deltaTime;
            yield return null;
        }

        // Stop the panther after charging for the specified duration
        rb.velocity = Vector2.zero;

        // Transition to idle by setting speed to 0 
        animator.SetFloat("speed", 0f);  

        // Disable the trail when the charge is complete
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        // Reset the charging state
        isCharging = false;
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

        // Clamp the panther's position within these adjusted boundaries
        float clampedX = Mathf.Clamp(transform.position.x, roomMinX, roomMaxX);
        float clampedY = Mathf.Clamp(transform.position.y, roomMinY, roomMaxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
