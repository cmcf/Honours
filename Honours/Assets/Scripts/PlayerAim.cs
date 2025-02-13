using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAim : MonoBehaviour
{
    public Transform weaponTransform;
    public Transform playerTransform;
    public Transform bulletSpawn;
    public GameObject crosshair;
    Animator animator;

    Vector3 defaultWeaponOffset = new Vector3(-0.161f, -0.033f, 0f);
    Vector3 weaponUpOffset = new Vector3(-0.1f, 0.12f, 0f);
    Vector3 weaponDownOffset = new Vector3(-0.161f, -0.033f, 0f);
    Vector3 targetWeaponPosition;

    public Vector2 aimDirection;


    public float weaponMoveSpeed = 10f; 

    void Start()
    {
        if (weaponTransform == null)
        {
            weaponTransform = transform;
        }

        animator = playerTransform.GetComponent<Animator>();

        targetWeaponPosition = defaultWeaponOffset;

        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            Mathf.Abs(Camera.main.transform.position.z)
        ));

        // Calculate aim direction (ignoring Z-axis)
        Vector3 direction = mousePosition - playerTransform.position;
        direction.z = 0f; 

        // Calculate the weapon angle for rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Flip player based on mouse position (for left/right aiming)
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

        // Set up aiming thresholds
        bool shouldFaceUp = direction.y > 0.5f;  // Upwards aiming threshold 
        bool shouldFaceDown = direction.y < -0.5f;  // Downwards aiming threshold
        bool isNeutralAim = !shouldFaceUp && !shouldFaceDown;

        // Update weapon position based on vertical aiming
        if (shouldFaceUp)
        {
            targetWeaponPosition = weaponUpOffset;
        }
        else if (shouldFaceDown)
        {
            targetWeaponPosition = weaponDownOffset;
        }
        else if (isNeutralAim)
        {
            targetWeaponPosition = defaultWeaponOffset;
        }

        // Smoothly move the weapon towards the target position using Lerp
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetWeaponPosition, weaponMoveSpeed * Time.deltaTime);

        // Update the crosshair position
        crosshair.transform.position = mousePosition;

        // Now handle the animation transitions for up/down aiming
        if (animator != null)
        {
            // Only set animMoveY to 1f when direction.y exceeds the threshold 
            if (direction.y > 0.5f) // Trigger the up animation when aiming up
            {
                animator.SetFloat("animMoveY", 1f);  // Aiming upwards
            }
            else if (direction.y < -0.5f)  // More lenient downward threshold
            {
                animator.SetFloat("animMoveY", -1f); // Aiming downwards
            }
            else
            {
                animator.SetFloat("animMoveY", 0f);  // Neutral aiming
            }
        }
    }

    void OnLook(InputValue value)
    {
        Vector2 inputDirection = value.Get<Vector2>();

        Debug.Log("Workign");
        // If using a controller (right stick), update aimDirection
        if (inputDirection.magnitude > 0.1f) // Ignore small stick movement
        {
            aimDirection = inputDirection.normalized;

            // Calculate angle for rotation
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            // Rotate weapon
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Flip player based on joystick aim direction
            playerTransform.localScale = (aimDirection.x > 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);

            Debug.Log(inputDirection);
        }
    }

}
