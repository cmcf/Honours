using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float moveSpeed = 3f;
    Vector2 moveDirection;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        // Apply movement to player 
        rb.velocity = moveDirection * moveSpeed;
    }


    void Update()
    {

    }

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();

    }
}
