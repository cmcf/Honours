using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldOrbit : MonoBehaviour
{

    public float speed = 5f;
    private Vector2 moveDirection;

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
        }
    }
}
