using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    public float _moveSpeed;
    private Vector2 _moveDirection;

    public InputActionReference moveAction;


    void Update()
    {
        _moveDirection = moveAction.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(_moveDirection.x * _moveSpeed, _rb.velocity.y, _moveDirection.y * _moveSpeed);
        // _rb.velocity = new Vector2(_moveDirection.x * _moveSpeed, _moveDirection.y * _moveSpeed);

        _rb.velocity = movement;
    }

    
}
