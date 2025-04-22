using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponUpgradePickup : MonoBehaviour
{
    WeaponUpgrade upgrade;
    InputAction interactAction;
    public Weapon weapon;  // The weapon the upgrade is for
    public List<WeaponUpgrade> possibleUpgrades; // List of possible upgrades
    public bool hasMaxUpgrade = false;
    bool playerInRange = false;
    void Start()
    {
        Player player = FindObjectOfType<Player>();
        weapon = player.currentWeapon;
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        // Start by choosing a random upgrade from the list
        if (!hasMaxUpgrade)
        {
            ChooseRandomUpgrade();
        }
    }

    void Update()
    {
        if (playerInRange && interactAction.triggered && !hasMaxUpgrade)
        {
            Player player = FindObjectOfType<Player>();

            if (player != null && upgrade != null)
            {
                // Apply the upgrade to the player's weapon
                player.ApplyWeaponUpgrade(upgrade);

                // Destroy the pickup object
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true; // Player is in range
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false; // Player is out of range
        }
    }

    void ChooseRandomUpgrade()
    {
        // Filter out the IncreaseSpreadCount upgrade if the player doesn't have a shotgun equipped
        if (weapon.weaponType != Weapon.WeaponType.SpreadShot)
        {
            possibleUpgrades.RemoveAll(upgrade => upgrade.upgradeType == WeaponUpgrade.UpgradeType.IncreaseSpreadCount);
        }

        if (possibleUpgrades.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleUpgrades.Count);
            WeaponUpgrade chosenUpgrade = possibleUpgrades[randomIndex];

            // Ensure the upgrade is applicable based on the weapon's stats
            if (chosenUpgrade.upgradeType == WeaponUpgrade.UpgradeType.IncreaseSpreadCount)
            {
                // Only apply if spread count is not maxed
                if (weapon.spreadCount >= weapon.maxSpreadCount)
                {
                    return; // Skip if spread count is maxed
                }
            }
            else if (chosenUpgrade.upgradeType == WeaponUpgrade.UpgradeType.IncreaseDamage)
            {
                // Skip if damage is already maxed
                if (weapon.minDamage >= weapon.maxMinDamage && weapon.maxDamage >= weapon.maxMaxDamage)
                {
                    return; // Skip if damage is already at max
                }
            }
            else if (chosenUpgrade.upgradeType == WeaponUpgrade.UpgradeType.IncreaseBulletSpeed)
            {
                // Skip if bullet speed is already maxed
                if (weapon.bulletSpeed >= weapon.maxBulletSpeed)
                {
                    return; // Skip if bullet speed is already maxed
                }
            }

            // Apply the chosen upgrade if it passed all checks
            upgrade = chosenUpgrade;
        }
    }
}
