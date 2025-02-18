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
        // Ensure the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the Switcher script to check the current character state
            Switcher switcher = FindObjectOfType<Switcher>();
            if (switcher == null)
            {
                Debug.LogError("Switcher script not found!");
                return;
            }

            // Only allow pickup if the player is in human form
            if (switcher.currentCharacterState == CharacterState.Player)
            {
                Debug.Log("Weapon picked up by player");
                other.GetComponent<Player>().PickupWeapon(selectedWeapon);

                // Destroy the pickup object after it is collected
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Cannot pick up weapon in wolf form");
            }
        }
    }


    public Weapon GetWeapon()
    {
        return selectedWeapon;  
    }
}
