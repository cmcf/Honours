using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    // Array of possible weapons to randomly pick from
    public Weapon[] possibleWeapons;
    SpriteRenderer spriteRenderer;
    // The selected random weapon
    Weapon selectedWeapon;  

    void Start()
    {
        // Randomly select a weapon from the array when the pickup is created
        selectedWeapon = possibleWeapons[Random.Range(0, possibleWeapons.Length)];
        spriteRenderer= GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedWeapon.weaponSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("Called");
            // Call the player's method to pick up the random weapon
            other.GetComponent<Player>().PickupWeapon(selectedWeapon);

            // Destroy the pickup object after it is collected
            Destroy(gameObject);
        }
    }

    public Weapon GetWeapon()
    {
        return selectedWeapon;  
    }
}
