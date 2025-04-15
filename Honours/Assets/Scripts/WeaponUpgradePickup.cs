using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradePickup : MonoBehaviour
{
    public WeaponUpgrade upgrade;
    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.ApplyWeaponUpgrade(upgrade); 
        }
    }
}
