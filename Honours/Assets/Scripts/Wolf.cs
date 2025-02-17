using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Wolf : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    [SerializeField] float moveSpeed = 6f;
    public bool canMoveWolf;

    public Vector2 moveDirection;
    Vector2 lastMoveDirection;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        Move();
    }

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
    }

    public void DisableInput()
    {
        moveDirection = Vector2.zero;
    }

    void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    void UpdateAnimation()
    {
        float speed = moveDirection.magnitude;
        Vector2 normalizedDirection = Vector2.zero;

        if (speed > 0)
        {
            normalizedDirection = moveDirection.normalized;
            lastMoveDirection = normalizedDirection;
        }

        // Update animation states
        animator.SetFloat("animMoveX", lastMoveDirection.x);
        animator.SetFloat("animMoveY", lastMoveDirection.y);
        animator.SetFloat("speed", speed);
    }

}
