using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Damage;

public class ShieldOrbit : MonoBehaviour
{
    public float speed = 5f;
    [SerializeField] int minDamage = 2;
    [SerializeField] int maxDamage = 8;
    int shieldDamage;
    [SerializeField] float knockbackForce = 5f;

    Vector2 moveDirection;

    void Start()
    {
        shieldDamage = Random.Range(minDamage, maxDamage);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Damage");

            // Get the IDamageable component
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Apply damage to the player
                damageable.Damage(shieldDamage);

            }
        }
    }

}
