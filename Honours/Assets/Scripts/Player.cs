using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Damage;

public class Player : MonoBehaviour
{
    public enum PlayerState { Normal, Enhanced }
    public PlayerState currentState = PlayerState.Normal;

    public GameObject bulletPrefab;
    public Transform spawnPoint;

    [SerializeField] float bulletSpeed = 10f;
    float lastFireTime = 0f;
    float stateTimer = 0f;
    float enhancedStateDuration = 9f;
    float enhancedBulletDamage = 15f;
    float defaultBulletDamage = 10f;
    [SerializeField] float fireDelay = 0.5f;

    PlayerMovement playerMovement; // Reference to PlayerMovement

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        StartCoroutine(RandomlyChangeState());
    }

    void Update()
    {
        // Manage the player state timer
        if (currentState == PlayerState.Enhanced)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ChangeState(PlayerState.Normal);
            }
        }
    }

    void OnFire()
    {
        if (Time.time > lastFireTime + fireDelay)
        {
            // Record the time of this shot
            lastFireTime = Time.time;
            GameObject projectile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

            // Pass the player's state to the bullet for damage adjustment
            Bullet bullet = projectile.GetComponent<Bullet>();
            if (bullet != null)
            {
                if (currentState == PlayerState.Enhanced)
                {
                    bullet.SetDamage(enhancedBulletDamage);
                }
                else
                {
                    bullet.SetDamage(defaultBulletDamage);
                }
            }
            // Move the bullet 
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            rb.velocity = spawnPoint.up * bulletSpeed;
        }
    }

    IEnumerator RandomlyChangeState()
    {
        while (true)
        {
            // Wait for a random interval
            float randomInterval = Random.Range(5, 15);
            yield return new WaitForSeconds(randomInterval);

            // Change to Enhanced state
            ChangeState(PlayerState.Enhanced);
            Debug.Log("Enhanced");

            // Wait until the enhanced state has passed then change back to normal
            yield return new WaitForSeconds(enhancedStateDuration);
            ChangeState(PlayerState.Normal);
            Debug.Log("Normal");
        }
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (newState == PlayerState.Enhanced)
        {
            stateTimer = enhancedStateDuration;
        }

        // Call the PlayerMovement script to change speed based on the new state
        playerMovement.ChangeSpeed(newState == PlayerState.Enhanced);
    }
}
