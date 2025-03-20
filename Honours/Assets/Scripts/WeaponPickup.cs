using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    // Array of possible weapons to randomly pick from
    public Weapon[] possibleWeapons;
    SpriteRenderer spriteRenderer;
    // The selected random weapon
    Weapon selectedWeapon;

    public float floatSpeed = 2f;  
    public float floatAmount = 0.1f;  

    Vector3 startPos;

    void Start()
    {
        // Get player's current weapon
        Player player = FindObjectOfType<Player>();
        Weapon currentWeapon = null;

        if (player != null)
        {
            currentWeapon = player.GetCurrentWeapon();
        }

        // Create a list excluding the current weapon
        List<Weapon> filteredWeapons = new List<Weapon>(possibleWeapons);
        if (currentWeapon != null)
        {
            filteredWeapons.RemoveAll(w => w.weaponName == currentWeapon.weaponName);
        }

        // Ensure there is at least one weapon to spawn
        if (filteredWeapons.Count > 0)
        {
            selectedWeapon = filteredWeapons[Random.Range(0, filteredWeapons.Count)];
        }
        else
        {
            selectedWeapon = possibleWeapons[Random.Range(0, possibleWeapons.Length)]; // Default to random if no other choice
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedWeapon.weaponSprite;
        startPos = transform.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Switcher switcher = FindObjectOfType<Switcher>();
            if (switcher == null) return;

            if (switcher.currentCharacterState == CharacterState.Player)
            {
                other.GetComponent<Player>().PickupWeapon(selectedWeapon);
                Destroy(gameObject);
            }
        }
    }

    public Weapon GetWeapon()
    {
        return selectedWeapon;
    }
}
