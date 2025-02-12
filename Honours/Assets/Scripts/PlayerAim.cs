using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public Transform weaponTransform;
    public Transform playerTransform;
    private PlayerMovement playerMovementScript;
    private Animator animator;

    private Vector3 defaultWeaponOffset = new Vector3(-0.3f, 0f, 0f);
    private Vector3 weaponUpOffset = new Vector3(-0.1f, 0.15f, 0f);
    private Vector3 weaponDownOffset = new Vector3(-0.2f, -0.15f, 0f);
    private Vector3 targetWeaponPosition;

    private bool isFacingUp = false;
    private bool isFacingDown = false;

    public float weaponMoveSpeed = 10f; 

    void Start()
    {
        if (weaponTransform == null)
        {
            weaponTransform = transform;
        }

        playerMovementScript = playerTransform.GetComponentInParent<PlayerMovement>();
        animator = playerTransform.GetComponent<Animator>();

        targetWeaponPosition = defaultWeaponOffset;
    }

    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            Mathf.Abs(Camera.main.transform.position.z)
        ));

        // Calculate aim direction
        Vector3 direction = mousePosition - playerTransform.position;
        direction.z = 0f; // Ignore Z axis for 2D

        // Calculate the angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Determine player sprite direction
        bool shouldFaceUp = direction.y > 1.1f;
        bool shouldFaceDown = direction.y < -0.8f;

        // Flip player and rotate weapon
        if (mousePosition.x > playerTransform.position.x)
        {
            playerTransform.localScale = new Vector3(-1, 1, 1);
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            playerTransform.localScale = new Vector3(1, 1, 1);
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        }

        // Update player animation based on movement
        playerMovementScript.UpdatePlayerAnimation(direction);

        // Update weapon position when the player sprite changes
        if (shouldFaceUp && !isFacingUp)
        {
            isFacingUp = true;
            isFacingDown = false;
            targetWeaponPosition = weaponUpOffset;
        }
        else if (shouldFaceDown && !isFacingDown)
        {
            isFacingDown = true;
            isFacingUp = false;
            targetWeaponPosition = weaponDownOffset;
        }
        else if (!shouldFaceUp && !shouldFaceDown)
        {
            isFacingUp = false;
            isFacingDown = false;
            targetWeaponPosition = defaultWeaponOffset;
        }

        // Smoothly move the weapon towards the target position using Lerp
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetWeaponPosition, weaponMoveSpeed * Time.deltaTime);
    }
}
