using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Damage;

public class Player : MonoBehaviour, IDamageable
{
    [Header("References")]
    
    public Enemy currentEnemy;
    PlayerAim playerAim;
    public SpriteRenderer weaponRenderer;
    Switcher characterSwitcher;
    PlayerHealth playerHealth;
    

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;
    Animator animator;

    [Header("Spawn Points")]
    public Transform spawnPoint;
    public Transform poisonBulletSpawn;
    public Transform beamSpawnPoint;

    [Header("Health")]
    
    [SerializeField] float health = 50f;

    [Header("Firing")]
    float lastFireTime = 0f;

    [Header("State")]
    public bool isDead = false; 

    public Weapon currentWeapon;
    bool isAutoFiring = false;
    [SerializeField] int numberOfDeaths = 0;

    private PlayerInput playerInput;
    private InputAction fireAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions["Fire"];

        // Subscribe to events
        fireAction.performed += OnFirePressed;
        fireAction.canceled += OnFireReleased;

        ResetWeaponStats();
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAim = GetComponentInChildren<PlayerAim>();

        characterSwitcher = FindObjectOfType<Switcher>();
        playerHealth = FindObjectOfType<PlayerHealth>();

        // Find the weapon sprite renderer inside the "Hands" object
        Transform hands = transform.Find("Hands");
        if (hands != null)
        {
            weaponRenderer = hands.Find("Weapon")?.GetComponent<SpriteRenderer>();
        }
        UpdateWeaponSprite();

        if (currentWeapon != null)
        {
            currentWeapon.InitialiseStats();
        }
    }



    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        fireAction.performed -= OnFirePressed;
        fireAction.canceled -= OnFireReleased;
    }

    void OnFirePressed(InputAction.CallbackContext context)
    {
        if (characterSwitcher.currentCharacterState == CharacterState.Wolf)
        {
            // Call the Wolf bite attack directly
            Wolf wolf = FindObjectOfType<Wolf>();
            if (wolf != null && !wolf.isBiting) // Check if wolf is not already attacking
            {
                wolf.BiteAttack(); // Trigger bite attack
            }
            return; // Stop further processing of the fire action
        }

        // Rest of the human form firing logic
        if (currentWeapon.weaponType == Weapon.WeaponType.Automatic ||currentWeapon.weaponType == Weapon.WeaponType.Beam)
        {
            isAutoFiring = true;
            StartCoroutine(AutoFireRifle());
        }
        else
        {
            OnFire();
        }
    }


    public void PickupWeapon(Weapon newWeapon)
    {
        EquipWeapon(newWeapon);
        ResetWeaponStats();
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        currentWeapon = newWeapon;
        UpdateWeaponSprite();
    }

    public void ApplyWeaponUpgrade(WeaponUpgrade upgrade)
    {
        if (currentWeapon != null)
        {
            // Find the WeaponUpgradePickup to pass to the Apply method
            WeaponUpgradePickup pickup = FindObjectOfType<WeaponUpgradePickup>();  // Or pass the pickup directly if needed

            upgrade.Apply(currentWeapon, pickup);  // Apply the upgrade and also show the UI feedback
        }
    }



    void UpdateWeaponSprite()
    {
        if (weaponRenderer != null && currentWeapon != null)
        {
            weaponRenderer.sprite = currentWeapon.weaponSprite;
            weaponRenderer.enabled = false;
            weaponRenderer.enabled = true;
        }
    }


    void OnFireReleased(InputAction.CallbackContext context)
    {
        // Stop auto-firing when fire button is released
        isAutoFiring = false; 
    }


    void OnFire()
    {

        if (Time.time > lastFireTime + currentWeapon.fireDelay)
        {
            lastFireTime = Time.time;
            Vector3 fireDirection = GetFireDirection();
            FireWeapon(fireDirection);
        }
    }


    Vector3 GetFireDirection()
    {
        Transform relevantSpawnPoint = spawnPoint;

        if (currentWeapon != null)
        {
            switch (currentWeapon.weaponType)
            {
                case Weapon.WeaponType.Poison:
                    relevantSpawnPoint = poisonBulletSpawn;
                    break;
                case Weapon.WeaponType.Beam:
                    relevantSpawnPoint = beamSpawnPoint;
                    break;
            }
        }

        Vector3 fireDirection;
        if (playerAim.usingController)
        {
            fireDirection = playerAim.aimDirection.normalized;
            if (fireDirection.magnitude < 0.1f)
            {
                fireDirection = Vector3.left;
            }
        }
        else
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Mathf.Abs(Camera.main.transform.position.z)
            ));
            fireDirection = (mousePosition - relevantSpawnPoint.position).normalized;
        }

        return fireDirection;
    }


    void FireWeapon(Vector3 direction)
    {
        switch (currentWeapon.weaponType)
        {
            case Weapon.WeaponType.Default:
                FireSingleBullet(direction);
                break;
            case Weapon.WeaponType.RapidFire:
                StartCoroutine(FireRapid(direction));
                break;
           case Weapon.WeaponType.SpreadShot:
                FireSpreadBullets(direction, currentWeapon.spreadCount, 30f);
                break;
            case Weapon.WeaponType.Ice:
                FireIceBullet(direction);
                break;
            case Weapon.WeaponType.Poison:
                FirePoisonBullet(direction);
                break;
            case Weapon.WeaponType.Beam:
                if (!isAutoFiring)
                {
                    isAutoFiring = true;
                    StartCoroutine(AutoFireBeam());
                }
                break;
        
            case Weapon.WeaponType.Automatic:
                if (!isAutoFiring)
                {
                    isAutoFiring = true;
                    StartCoroutine(AutoFireRifle());
                }
                break;
        }
    }


    void FireBeamProjectile(Vector3 direction)
    {
        direction.Normalize();
        Vector3 bulletSpawnPosition = beamSpawnPoint.position;
        GameObject projectile = Instantiate(currentWeapon.bulletPrefab, bulletSpawnPosition, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeapon.bulletSpeed;
        }
        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(currentWeapon.GetRandomDamage());
            bullet.isIce = false;
        }
    }

    void FireSingleBullet(Vector3 direction)
    {
        direction.Normalize();
        Vector3 bulletSpawnPosition = spawnPoint.position + direction * 0.5f;
        GameObject projectile = Instantiate(currentWeapon.bulletPrefab, bulletSpawnPosition, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeapon.bulletSpeed;
        }
        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(currentWeapon.GetRandomDamage());
            bullet.isIce = false;
        }
    }

    void FirePoisonBullet(Vector3 direction)
    {
        direction.Normalize();
        Vector3 bulletSpawnPosition = poisonBulletSpawn.position;
        GameObject projectile = Instantiate(currentWeapon.bulletPrefab, bulletSpawnPosition, Quaternion.identity);

        projectile.transform.position = bulletSpawnPosition;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeapon.bulletSpeed;
        }

        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(currentWeapon.GetRandomDamage());
            bullet.isPoison = true; 
        }
    }


    IEnumerator FireRapid(Vector3 direction)
    {
        for (int i = 0; i < 3; i++)
        {
            FireSingleBullet(direction);
            yield return new WaitForSeconds(currentWeapon.fireDelay / 3);
        }
    }

    IEnumerator AutoFireBeam()
    {
        while (isAutoFiring)
        {
            if (Time.time > lastFireTime + currentWeapon.fireDelay)
            {
                lastFireTime = Time.time;
                FireBeamProjectile(GetFireDirection());
            }
            yield return null; 
        }
    }

    IEnumerator AutoFireRifle()
    {
        while (isAutoFiring)
        {
            if (Time.time > lastFireTime + currentWeapon.fireDelay)
            {
                lastFireTime = Time.time;
                FireSingleBullet(GetFireDirection());
            }
            yield return null;
        }
    }

    void FireSpreadBullets(Vector3 direction, int bulletCount, float spreadAngle)
    {
        float angleStep = spreadAngle / (bulletCount - 1);
        float startAngle = -spreadAngle / 2;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector3 bulletDirection = Quaternion.Euler(0, 0, angle) * direction;
            FireSingleBullet(bulletDirection);
        }
    }

    void FireIceBullet(Vector3 direction)
    {
        direction = direction.normalized;
        GameObject iceBullet = Instantiate(currentWeapon.bulletPrefab, spawnPoint.position, Quaternion.identity);
        iceBullet.transform.right = direction;
        Rigidbody2D rb = iceBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * currentWeapon.bulletSpeed;
        }
        Bullet bullet = iceBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(currentWeapon.GetRandomDamage());
            bullet.isIce = true;
        }
    }

    public void Damage(int damage)
    {
        if (isDead) { return; }
        // Current health is decreased by the damage received
        playerHealth.TakeDamage(damage);
    }

    public void Die()
    {
        // Only enter death state once
        if (isDead) { return; }

        // Disable player move input
        playerMovement.enabled = false;

        // Disable aim ability
        playerAim.enabled = false;

        // Stop movement to prevent sliding
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Set player to death state
        isDead = true;

        // Increase death counter
        numberOfDeaths++;

        ResetWeaponStats();

        // Play death animation
        animator.SetTrigger("isDead");
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon; // Replace with your actual variable for the current weapon
    }

    public bool IsWeaponMaxedOut()
    {
        // Check if the weapon has reached its max upgrades for all stats
        if (currentWeapon.spreadCount >= currentWeapon.maxSpreadCount &&
            currentWeapon.bulletSpeed >= currentWeapon.maxBulletSpeed &&
            currentWeapon.minDamage >= currentWeapon.maxMinDamage &&
            currentWeapon.maxDamage >= currentWeapon.maxMaxDamage)
        {
            return true; // Weapon is maxed out
        }

        return false; // Weapon is not maxed out
    }

    void ResetWeaponStats()
    {
        currentWeapon.spreadCount = currentWeapon.baseSpreadCount;
        currentWeapon.bulletSpeed = currentWeapon.baseBulletSpeed;
        currentWeapon.minDamage = currentWeapon.baseMinDamage;
        currentWeapon.maxDamage = currentWeapon.baseMaxDamage;
    }

}
