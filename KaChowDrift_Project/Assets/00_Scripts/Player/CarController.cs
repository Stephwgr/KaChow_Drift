using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;




public class CarController : MonoBehaviour
{
    public WheelColliders _wheelColliders;
    public WheelMeshes _wheelMeshes;

    public float _gasInput;
    public float _backInput;
    public Vector2 _steeringInput;

    public InputActionReference moveAction;

    public InputActionReference gasAction;
    public InputActionReference backAction;



    private void Update()
    {
        ApplyWheelPositions();
        CheckInput();

    }

    void CheckInput()
    {
        _steeringInput = moveAction.action.ReadValue<Vector2>();
        _gasInput = gasAction.action.ReadValue<float>();
        _backInput = - backAction.action.ReadValue<float>(); 

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

