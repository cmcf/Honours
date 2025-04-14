using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public Sprite weaponSprite;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 2f;
    public float fireDelay = 0.5f;

    public int minDamage;  // Minimum damage
    public int maxDamage; // Maximum damage

    public enum WeaponType
    {
        Default,    // Fires a single bullet
        RapidFire,  // Fires bullets with a faster rate
        SpreadShot, // Fires multiple bullets in a spread
        Ice,        // Fires an ice projectile
        Poison,
        Automatic     // Autofire projectiles
    }

    public int GetRandomDamage()
    {
        return Random.Range(minDamage, maxDamage + 1);
    }
}
