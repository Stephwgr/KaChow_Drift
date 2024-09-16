using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;



public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    private Vector2 _moveDirection;
    private float _accelerate;
    private bool _isDrifting = false;


    [BoxGroup("Management Speed")]
    [SerializeField] private float _accelerationForce = 50f;
    [SerializeField] private float _steeringForce = 100f;
    [SerializeField] private float _maxSpeed = 20f;

    [Header("Settings Drift")]
    [SerializeField] private float _driftFactor = 0.95f;
    [SerializeField] private float _driftFactorWhileDrifting = 0.7f;
    // public float _moveSpeed;

    [BoxGroup("Input Action")]
    public InputActionReference moveAction;
    public InputActionReference accelerateAction;
    public InputActionReference driftAction;

    private void Start()
    {
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        _moveDirection = moveAction.action.ReadValue<Vector2>();
        _accelerate = accelerateAction.action.ReadValue<float>();
        _isDrifting = driftAction.action.IsPressed();



    }

    private void FixedUpdate()
    {
        // Vector3 movement = new Vector3(_moveDirection.x * _moveSpeed, _rb.velocity.y, _moveDirection.y * _moveSpeed);
        // _rb.velocity = movement; //direction axe X et Z



        Accelerate();
        Steer();
        ApplyDrift();
        

    }

    private void Accelerate()
    {
        //Appliquer la force d'accéleration uniquement en fonction de la pression de la touche gachette droite
        Vector3 forwardForce = transform.forward * _accelerate * _accelerationForce;

        //Limiter la vitesse maximal
        if (_rb.velocity.magnitude < _maxSpeed)
        {
            _rb.AddForce(forwardForce, ForceMode.Acceleration);
        }
    }

    private void Steer()
    {
        // Rotation de la voiture basée sur l'entrée gauche/droite (Axe X)
        float turn = _moveDirection.x * _steeringForce * Time.fixedDeltaTime;
        _rb.rotation = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y + turn, 0f);
    }

    private void ApplyDrift()
    {
        float currentDriftFactor = _isDrifting ? _driftFactorWhileDrifting : _driftFactor;
        Vector3 driftVelocity = Vector3.Project(_rb.velocity, transform.forward);
        _rb.velocity = Vector3.Lerp(_rb.velocity, driftVelocity, currentDriftFactor * Time.fixedDeltaTime);
    }

    private void ApplyDownwardForce()
    {
        _rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
    }


}
