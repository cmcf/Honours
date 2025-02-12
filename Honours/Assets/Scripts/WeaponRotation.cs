using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponRotation : MonoBehaviour
{
    void Update()
    {
        RotateWeaponToMouse();
    }

    void RotateWeaponToMouse()
    {
        // Ensure the camera exists
        if (Camera.main == null) return;

        // Get the mouse position in world coordinates
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Calculate the direction from the weapon to the mouse
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;

        // Calculate the rotation angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
