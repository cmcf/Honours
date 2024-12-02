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

    void Update()
    {
        
    }
}
