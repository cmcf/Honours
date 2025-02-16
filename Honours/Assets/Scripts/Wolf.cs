using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Wolf : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float moveSpeed = 6f;
    public bool canMoveWolf;

    public Vector2 moveDirection;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    void OnMove(InputValue value)
    {
        // Get movement input
        moveDirection = value.Get<Vector2>();
    }

    public void DisableInput()
    {
        moveDirection = Vector2.zero;
    }
}
