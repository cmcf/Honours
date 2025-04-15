using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public Sprite weaponSprite;
    public GameObject bulletPrefab;

    // Default values
    public float baseBulletSpeed = 10f;
    public float baseBulletLifetime = 2f;
    public float baseFireDelay = 0.5f;
    public int baseSpreadCount = 2;
    public int baseMinDamage;  // Minimum damage
    public int baseMaxDamage; // Maximum damage

    // Current values
    public float bulletSpeed;
    public float bulletLifetime;
    public int spreadCount;
    public int minDamage;  // Minimum damage
    public int maxDamage; // Maximum damage


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
