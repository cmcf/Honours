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
        // Create a filtered list of upgrades that are valid for the current weapon
        List<WeaponUpgrade> validUpgrades = new List<WeaponUpgrade>();

        foreach (WeaponUpgrade upgradeOption in possibleUpgrades)
        {
            // Skip SpreadCount if not using a shotgun
            if (upgradeOption.upgradeType == WeaponUpgrade.UpgradeType.IncreaseSpreadCount &&
                weapon.weaponType != Weapon.WeaponType.SpreadShot)
            {
                continue;
            }

            // Check maxed conditions
            if (upgradeOption.upgradeType == WeaponUpgrade.UpgradeType.IncreaseSpreadCount &&
                weapon.spreadCount >= weapon.maxSpreadCount)
            {
                continue;
            }

            if (upgradeOption.upgradeType == WeaponUpgrade.UpgradeType.IncreaseDamage &&
                weapon.minDamage >= weapon.maxMinDamage && weapon.maxDamage >= weapon.maxMaxDamage)
            {
                continue;
            }

            if (upgradeOption.upgradeType == WeaponUpgrade.UpgradeType.IncreaseBulletSpeed &&
                weapon.bulletSpeed >= weapon.maxBulletSpeed)
            {
                continue;
            }

            if (upgradeOption.upgradeType == WeaponUpgrade.UpgradeType.IncreasedFireRate &&
                weapon.fireDelay <= weapon.minFireRate)
            {
                continue;
            }

            // If it passed all checks, add to valid list
            validUpgrades.Add(upgradeOption);
        }

        if (validUpgrades.Count > 0)
        {
            int randomIndex = Random.Range(0, validUpgrades.Count);
            upgrade = validUpgrades[randomIndex];
        }
        else
        {
            upgrade = null; // All upgrades are maxed
        }
    }
}
