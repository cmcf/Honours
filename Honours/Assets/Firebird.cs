using TMPro;
using System.Collections;
using UnityEngine;

public class Firebird : MonoBehaviour
{
    Transform player;

    public float moveSpeed = 5f; // Movement speed
    public float pauseDuration = 1.5f; // Time spent at each position
    [SerializeField] float moveDuration = 2f; // Time to move between sides
    Rigidbody2D rb;
    Animator animator;

    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 5f;
    public float fireRate = 2f; // Time between shots

    Vector2 targetPosition;
    bool isMoving = false;

    public Room currentRoom;
    Coroutine firingCoroutine;

    void Start()
    {
        // References 
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        StartCoroutine(BossRoutine());
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Keep the firebird within the room by clamping its position
        if (currentRoom != null)
        {
            ClampPosition();
        }
    }

    void MoveInDirection(Vector2 direction)
    {
        rb.velocity = direction.normalized * moveSpeed;
    }

    // Updates animation direction based on velocity
    void UpdateAnimationDirection(Vector2 velocity)
    {
        if (velocity.sqrMagnitude > 0.01f) // Only update if there's movement
        {
            animator.SetFloat("velocityX", velocity.x);
            animator.SetFloat("velocityY", velocity.y);
        }

        animator.SetFloat("speed", velocity.magnitude); // Set speed for animation transitions
    }

    IEnumerator BossRoutine()
    {

        while (true)
        {
            // Move to the four positions: top, bottom, left, and right
            Vector2[] positions = new Vector2[]
            {
                new Vector2(currentRoom.GetRoomCentre().x, currentRoom.GetRoomCentre().y + currentRoom.height / 2 - 1f), // Top
                new Vector2(currentRoom.GetRoomCentre().x, currentRoom.GetRoomCentre().y - currentRoom.height / 2 + 1f), // Bottom
                new Vector2(currentRoom.GetRoomCentre().x - currentRoom.width / 2 + 1f, currentRoom.GetRoomCentre().y), // Left
                new Vector2(currentRoom.GetRoomCentre().x + currentRoom.width / 2 - 1f, currentRoom.GetRoomCentre().y)  // Right
            };

            foreach (var position in positions)
            {
                Debug.Log($"Moving to position: {position}");

                targetPosition = position;

                // Move to the current position and fire at the player
                yield return MoveToSetPosition(position);

                // Start firing at player (with a delay before firing)
                firingCoroutine = StartCoroutine(FireAtPlayerRoutine());

                // Fire for a period then move again
                yield return new WaitForSeconds(pauseDuration);

                // Stop firing and move to next position
                StopCoroutine(firingCoroutine);
            }
        }
    }

    IEnumerator MoveToSetPosition(Vector2 target)
    {

        isMoving = true;
        Vector2 startPosition = transform.position;

        // Calculate direction for animation
        Vector2 moveDirection = (target - startPosition).normalized;
        UpdateAnimationDirection(moveDirection);

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(startPosition, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;

        // Stop movement animation when reaching the position
        UpdateAnimationDirection(Vector2.zero);
    }

    // Clamps the firebird's position to stay within the room boundaries
    void ClampPosition()
    {
        float margin = 2f;

        // Calculate the room's boundaries with margin
        float roomMinX = currentRoom.GetRoomCentre().x - (currentRoom.width / 2) + margin;
        float roomMaxX = currentRoom.GetRoomCentre().x + (currentRoom.width / 2) - margin;
        float roomMinY = currentRoom.GetRoomCentre().y - (currentRoom.height / 2) + margin;
        float roomMaxY = currentRoom.GetRoomCentre().y + (currentRoom.height / 2) - margin;

        // Clamp the firebird's position within these adjusted boundaries
        float clampedX = Mathf.Clamp(transform.position.x, roomMinX, roomMaxX);
        float clampedY = Mathf.Clamp(transform.position.y, roomMinY, roomMaxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void FireSpreadAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null) return;

        int numberOfProjectiles = 5; // Number of spread bullets
        float spreadAngle = 30f; // How wide the spread should be
        float angleStep = spreadAngle / (numberOfProjectiles - 1);

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = -spreadAngle / 2 + angleStep * i; // Calculate angle for each bullet

            Vector2 fireDirection = (player.position - firePoint.position).normalized;
            fireDirection = RotateVector(fireDirection, angle); // Rotate the direction by the calculated angle

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();

            if (projRb != null)
            {
                projRb.velocity = fireDirection * projectileSpeed;
            }
        }
    }

    // Helper function to rotate a vector by a given angle
    Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y);
    }

    // Coroutine to fire at the player
    IEnumerator FireAtPlayerRoutine()
    {

        // Ensure the Firebird is facing the player
        UpdateAnimationDirectionTowardsPlayer();

        // Keep firing at the player
        while (true)
        {
            FireSpreadAtPlayer();  // Fire a projectile at the player
            yield return new WaitForSeconds(fireRate); // Wait for the next shot
        }
    }

    // Update the animation direction to face the player when the Firebird stops moving
    void UpdateAnimationDirectionTowardsPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        animator.SetFloat("velocityX", directionToPlayer.x);
        animator.SetFloat("velocityY", directionToPlayer.y);
        animator.SetFloat("speed", 0f); // Stop the movement animation when still
    }
}
