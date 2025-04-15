using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public Sprite weaponSprite;
    public GameObject bulletPrefab;

    [Header("Base Values")]
    public float baseBulletSpeed = 10f;
    public float baseBulletLifetime = 2f;
    public float fireDelay = 0.5f;
    public int baseSpreadCount = 2;
    public int baseMinDamage;  // Minimum damage
    public int baseMaxDamage; // Maximum damage

    [Header("Current Values")]
    public float bulletSpeed;
    public float bulletLifetime;
    public int spreadCount;
    public int minDamage;  // Minimum damage
    public int maxDamage; // Maximum damage

    [Header("Max Values")]
    public int maxSpreadCount = 5;
    public int maxMinDamage = 18;
    public int maxMaxDamage = 28;
    public float maxBulletSpeed = 25;
    public float maxBulletLifeTime = 2.5f;



    public enum WeaponType
    {
        Default,    // Fires a single bullet
        RapidFire,  // Fires bullets with a faster rate
        SpreadShot, // Fires multiple bullets in a spread
        Ice,        // Fires an ice projectile
        Poison,     // Fires poisonous projectiles
        Beam,       // Single beam projectile
        Automatic     // Autofire projectiles
    }

    public void InitialiseStats()
    {
        bulletSpeed = baseBulletSpeed;
        spreadCount = baseSpreadCount;
        minDamage = baseMinDamage;
        maxDamage = baseMaxDamage;
    }

    public int GetRandomDamage()
    {
        return Random.Range(baseMinDamage, baseMaxDamage + 1);
    }
}
