using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    float bulletLife = 10f;
    void Start()
    {
        Destroy(gameObject, bulletLife);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroys bullet if it hits a wall
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
