using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public Transform weaponTransform;
    public Transform playerMovement;
    PlayerMovement playerMovementScript;


    void Start()
     {

        if (weaponTransform == null)
        {
            weaponTransform = transform;
        }

        playerMovementScript = playerMovement.GetComponentInParent<PlayerMovement>();
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
        Vector3 direction = mousePosition - playerMovement.position;
        direction.z = 0f; // Ensure we ignore Z axis (2D movement)

        // Calculate the angle between the direction and the X axis
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Flip the player based on mouse position for visual direction
        if (mousePosition.x > playerMovement.position.x)
        {
            playerMovement.localScale = new Vector3(-1, 1, 1);  // Player faces right
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle); // Aim the weapon
            playerMovementScript.UpdatePlayerAnimation(direction); // Update animation values
        }
        else
        {
            playerMovement.localScale = new Vector3(1, 1, 1);   // Player faces left
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle + 180f); // Aim the weapon
            playerMovementScript.UpdatePlayerAnimation(direction); // Update animation values
        }
    }

}

