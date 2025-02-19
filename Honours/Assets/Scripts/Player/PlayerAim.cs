using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public Transform weaponTransform;
    public Transform playerTransform;
    public Transform bulletSpawn;
    public GameObject crosshair;

    PlayerInput playerInput;
    Animator animator;

    Vector3 defaultWeaponOffset = new Vector3(-0.161f, -0.033f, 0f);
    Vector3 weaponUpOffset = new Vector3(-0.1f, 0.12f, 0f);
    Vector3 weaponDownOffset = new Vector3(-0.161f, -0.033f, 0f);
    Vector3 targetWeaponPosition;

    public Vector2 aimDirection;
    public bool usingController = false;

    public float weaponMoveSpeed = 10f;
    public float crosshairDistance = 4f; 

    public Vector2 lastFireDirection;

    void Start()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        playerInput.currentActionMap.Enable();
        playerInput.actions["Look"].performed += ctx => OnLook(ctx);

        animator = playerTransform.GetComponent<Animator>();
        targetWeaponPosition = defaultWeaponOffset;

        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 direction = Aim();

        UpdatePlayerAnimations(direction);
    }

    Vector3 Aim()
    {
        Vector3 aimTarget;

        if (usingController)
        {
            // Controller aiming: keep crosshair at a fixed distance
            if (aimDirection.magnitude > 0.1f)
            {
                aimTarget = playerTransform.position + (Vector3)aimDirection.normalized * crosshairDistance;
            }
            else
            {
                aimTarget = playerTransform.position + new Vector3(crosshairDistance, 0, 0); // Default right
            }
        }
        else
        {
            // Mouse aiming: follow mouse position with no fixed distance
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Mathf.Abs(Camera.main.transform.position.z)
            ));

            // Directly set the crosshair to the mouse position (without distance restriction)
            aimTarget = mouseWorldPosition;
        }

        // Calculate aim direction
        Vector3 direction = (aimTarget - playerTransform.position).normalized;

        // Calculate weapon rotation angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Flip player and weapon rotation
        if (aimTarget.x > playerTransform.position.x)
        {
            playerTransform.localScale = new Vector3(-1, 1, 1);
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            playerTransform.localScale = new Vector3(1, 1, 1);
            weaponTransform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        }

        // Store last valid fire direction
        lastFireDirection = direction;

        // Update crosshair position (fixed distance or direct mouse position)
        crosshair.transform.position = aimTarget;
        return direction;
    }


    void UpdatePlayerAnimations(Vector3 direction)
    {
        // Animation transitions
        if (animator != null)
        {
            if (direction.y > 0.5f)
            {
                animator.SetFloat("animMoveY", 1f);
            }
            else if (direction.y < -0.5f)
            {
                animator.SetFloat("animMoveY", -1f);
            }
            else
            {
                animator.SetFloat("animMoveY", 0f);
            }
        }
    }

    void OnLook(InputAction.CallbackContext context)
    {
        aimDirection = context.ReadValue<Vector2>();

        if (aimDirection.magnitude > 0.1f)
        {
            usingController = true;
        }
        else
        {
            usingController = false;
        }
    }
}
