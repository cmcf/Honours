using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public Transform weaponTransform;
    public Transform playerTransform;
    PlayerMovement playerMovementScript;

    void Start()
    {
        if (weaponTransform == null)
        {
            weaponTransform = transform;
        }

        playerMovementScript = playerTransform.GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            Mathf.Abs(Camera.main.transform.position.z)
        ));

        // Calculate the direction from the player to the mouse
        Vector3 direction = mousePosition - playerTransform.position;
        direction.z = 0f; // Ensure we ignore Z axis (2D movement)

        // Calculate the angle between the direction and the X axis
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Flip the player based on mouse position for visual direction
        if (mousePosition.x > playerTransform.position.x)
        {
            playerTransform.localScale = new Vector3(-1, 1, 1);  // Player faces right
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle); // Aim the weapon
            playerMovementScript.UpdatePlayerAnimation(direction); // Update animation values
        }
        else
        {
            playerTransform.localScale = new Vector3(1, 1, 1);   // Player faces left
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle + 180f); // Aim the weapon
            playerMovementScript.UpdatePlayerAnimation(direction); // Update animation values
        }

        // Adjust weapon position based on player movement
        Vector2 moveDirection = playerMovementScript.moveDirection; 
        playerMovementScript.AdjustWeaponPosition(moveDirection);


    }

public void AdjustWeaponPosition(Vector2 direction)
    {
        // Adjust weapon position only based on vertical direction (up or down)
        if (direction.y > 0) // Facing up
        {
            weaponTransform.localPosition = new Vector3(-0.137f, 0.195f, 0); // Fixed weapon position when facing up
        }
        else if (direction.y < 0) // Facing down
        {
            weaponTransform.localPosition = new Vector3(-0.147f, -0.134f, 0); // Fixed weapon position when facing down
        }
        else // Horizontal (left/right) - no change in position
        {
            weaponTransform.localPosition = new Vector3(-0.335f, 0f, 0); // Default horizontal position
        }
    }
}
