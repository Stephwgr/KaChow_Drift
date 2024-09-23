using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;




public class CarController : MonoBehaviour
{
    private Rigidbody _rbPlayer;
    public WheelColliders _wheelColliders;
    public WheelMeshes _wheelMeshes;
    public WheelParticles _wheelParticles;

    public float _gasInput;
    // public float _backInput;
    public float _steeringInput;
    public float _brakeInput;

    public float _motorPower;
    public float _breakPower;
    private float _slipAngle; //angle de glissement
    private float _speed;
    public AnimationCurve _steeringCurve;

    public GameObject _smokePrefab;

    // public InputActionReference moveActionVector2;

    public InputActionReference moveActionFloat;
    public InputActionReference gasAction;
    // public InputActionReference backAction;
    public InputActionReference brakeAction;


    private void Start()
    {
        _rbPlayer = gameObject.GetComponent<Rigidbody>();
        InstantiateSmoke();
        
    }


    private void Update()
    {
        _speed = _rbPlayer.velocity.magnitude;

        CheckInput();
        ApplyWheelPositions();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();


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

    void InstantiateSmoke()
    {
        _wheelParticles.FRWheel = Instantiate(_smokePrefab, _wheelColliders.FRWheel.transform.position - Vector3.up * _wheelColliders.FRWheel.radius, Quaternion.identity, _wheelColliders.FRWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.FLWheel = Instantiate(_smokePrefab, _wheelColliders.FLWheel.transform.position- Vector3.up * _wheelColliders.FLWheel.radius, Quaternion.identity, _wheelColliders.FLWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.RRWheel = Instantiate(_smokePrefab, _wheelColliders.RRWheel.transform.position- Vector3.up * _wheelColliders.RRWheel.radius, Quaternion.identity, _wheelColliders.RRWheel.transform)
            .GetComponent<ParticleSystem>();
        _wheelParticles.RLWheel = Instantiate(_smokePrefab, _wheelColliders.RLWheel.transform.position- Vector3.up * _wheelColliders.RLWheel.radius, Quaternion.identity, _wheelColliders.RLWheel.transform)
            .GetComponent<ParticleSystem>();
    }

    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        _wheelColliders.FRWheel.GetGroundHit(out wheelHits[0]);
        _wheelColliders.FLWheel.GetGroundHit(out wheelHits[1]);
        _wheelColliders.RRWheel.GetGroundHit(out wheelHits[2]);
        _wheelColliders.RLWheel.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.3f;

        if((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            _wheelParticles.FRWheel.Play();
        }
        else
        {
            _wheelParticles.FRWheel.Stop();

        }
        if((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            _wheelParticles.FLWheel.Play();
        }
        else
        {
            _wheelParticles.FLWheel.Stop();

        }
        if((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            _wheelParticles.RRWheel.Play();
        }
        else
        {
            _wheelParticles.RRWheel.Stop();

        }
        if((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            _wheelParticles.RLWheel.Play();
        }
        else
        {
            _wheelParticles.RLWheel.Stop();

        }
    }

    void ApplyBrake()
    {
        _wheelColliders.FRWheel.brakeTorque = _brakeInput * _breakPower * 0.7F;
        _wheelColliders.FLWheel.brakeTorque = _brakeInput * _breakPower * 0.7F;
        _wheelColliders.RRWheel.brakeTorque = _brakeInput * _breakPower * 0.4F;
        _wheelColliders.RLWheel.brakeTorque = _brakeInput * _breakPower * 0.4F;
    }

    void ApplySteering()
    {
        float steeringAngle = _steeringInput * _steeringCurve.Evaluate(_speed);

        // Contre-braquage basé sur la vitesse et la dérive
        float counterSteer = Vector3.SignedAngle(transform.forward, _rbPlayer.velocity, Vector3.up);
        counterSteer = Mathf.Clamp(counterSteer, -20f, 20f);
        steeringAngle += counterSteer;

        // if(_gasInput < 0)
        // {
        //     steeringAngle = - steeringAngle;
        // }

        // steeringAngle += Vector3.SignedAngle(transform.forward, _rbPlayer.velocity + transform.forward, Vector3.up);
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);

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

[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;
}

