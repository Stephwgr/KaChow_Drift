using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;




public class CarController : MonoBehaviour
{
    public Rigidbody _rbPlayer;
    public WheelColliders _wheelColliders;
    public WheelMeshes _wheelMeshes;

    public float _gasInput;
    // public float _backInput;
    public float _steeringInput;
    public float _brakeInput;

    public float _motorPower;
    public float _breakPower;
    private float _slipAngle; //angle de glissement
    private float _speed;
    public AnimationCurve _steeringCurve;

    // public InputActionReference moveActionVector2;

    public InputActionReference moveActionFloat;
    public InputActionReference gasAction;
    // public InputActionReference backAction;
    public InputActionReference brakeAction;


    private void Start()
    {
        _rbPlayer = gameObject.GetComponent<Rigidbody>();
    }


    private void Update()
    {
        _speed = _rbPlayer.velocity.magnitude;

        CheckInput();
        ApplyWheelPositions();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();


    }

    void CheckInput()
    {
        _steeringInput = moveActionFloat.action.ReadValue<float>();
        _gasInput = gasAction.action.ReadValue<float>();
        // _brakeInput = brakeAction.action.ReadValue<float>();
        // _backInput = -backAction.action.ReadValue<float>();

        _slipAngle = Vector3.Angle(transform.forward, _rbPlayer.velocity - transform.forward);

        if (_slipAngle < 120f)
        {
            if (_gasInput < 0)
            {
                _brakeInput = Mathf.Abs(_gasInput);
                _gasInput = 0;
            }
            else
            {
                _brakeInput = 0f;
            }
        }
        else
            {
                _brakeInput = 0f;
            }

    }

    void ApplyBrake()
    {
        _wheelColliders.FRWheel.brakeTorque = _brakeInput * _breakPower * 0.7F;
        _wheelColliders.FLWheel.brakeTorque = _brakeInput * _breakPower * 0.7F;
        _wheelColliders.RRWheel.brakeTorque = _brakeInput * _breakPower * 0.3F;
        _wheelColliders.RLWheel.brakeTorque = _brakeInput * _breakPower * 0.3F;
    }

    void ApplySteering()
    {
        float steeringAngle = _steeringInput * _steeringCurve.Evaluate(_speed);
        _wheelColliders.FRWheel.steerAngle = steeringAngle;
        _wheelColliders.FLWheel.steerAngle = steeringAngle;
    }

    void ApplyMotor()
    {
        _wheelColliders.RRWheel.motorTorque = _motorPower * _gasInput;
        _wheelColliders.RLWheel.motorTorque = _motorPower * _gasInput;
    }


    void ApplyWheelPositions()
    {
        UpdateWheel(_wheelColliders.FRWheel, _wheelMeshes.FRWheel);
        UpdateWheel(_wheelColliders.FLWheel, _wheelMeshes.FLWheel);
        UpdateWheel(_wheelColliders.RRWheel, _wheelMeshes.RRWheel);
        UpdateWheel(_wheelColliders.RLWheel, _wheelMeshes.RLWheel);

    }

    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }


}

[System.Serializable]
public class WheelColliders
{
    // FR -> Front Right / FL -> Front Left // RL RearLeft (Rear = Arrière) // RR etc...
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}

[System.Serializable]
public class WheelMeshes
{
    // FR -> Front Right / FL -> Front Left // RL RearLeft (Rear = Arrière) // RR etc...
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}

