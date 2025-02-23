using UnityEngine;

public class Firebird : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float changeDirectionTime = 2f; // Time before changing direction
    Rigidbody2D rb;
    Animator animator;
    float directionChangeTimer;

    public Room currentRoom;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        directionChangeTimer = changeDirectionTime; 
    }

    // Update is called once per frame
    void Update()
    {
        // Count down the direction change timer
        directionChangeTimer -= Time.deltaTime;

        // If it's time to change direction, update movement
        if (directionChangeTimer <= 0)
        {
            // Randomize new direction
            Vector2 newDirection = GetRandomDirection();
            MoveInDirection(newDirection);
            directionChangeTimer = changeDirectionTime; 
        }

        UpdateAnimationDirection(rb.velocity);

        // Update speed parameter based on the magnitude of the velocity
        float speed = rb.velocity.magnitude;
        animator.SetFloat("speed", speed);

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

    Vector2 GetRandomDirection()
    {
        // Randomize movement direction 
        float randX = Random.Range(-1f, 1f);
        float randY = Random.Range(-1f, 1f);
        return new Vector2(randX, randY);
    }

    // Updates animation direction based on velocity
    void UpdateAnimationDirection(Vector2 velocity)
    {
        animator.SetFloat("velocityX", velocity.x);
        animator.SetFloat("velocityY", velocity.y);
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

}
