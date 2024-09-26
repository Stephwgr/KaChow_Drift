using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;


public class DetectCollision : MonoBehaviour
{
    public MMF_Player _mmfPlayer;

    public float _raycastDistance;
    public bool isColliding = false;

    public Vector3 centerOffset = new Vector3(0f, 0.5f, 2f);
    public Vector3 leftOffset = new Vector3(-0.5f, 0.5f, 2f);
    public Vector3 rightOffset = new Vector3(0.5f, 0.5f, 2f);

    [Header("DATA TIMER")]
    [SerializeField] private float _timeBtwDetections;
    private float _lastDetection = 0f;


    private void Update()
    {
        DetectWall();
    }

    void DetectWall()
    {
        Vector3 centerRay = transform.position + transform.TransformDirection(centerOffset);
        Vector3 leftRay = transform.position + transform.TransformDirection(leftOffset);
        Vector3 rightRay = transform.position + transform.TransformDirection(rightOffset);

        RaycastHit hitCenter, hitLeft, hitRight;

        bool hitCenterDetected = Physics.Raycast(centerRay, transform.forward, out hitCenter, _raycastDistance);
        bool hitLeftDetected = Physics.Raycast(leftRay, transform.forward, out hitLeft, _raycastDistance);
        bool hitRightDetected = Physics.Raycast(rightRay, transform.forward, out hitRight, _raycastDistance);

        Debug.DrawRay(centerRay, transform.forward * _raycastDistance, Color.green);
        Debug.DrawRay(leftRay, transform.forward * _raycastDistance, Color.green);
        Debug.DrawRay(rightRay, transform.forward * _raycastDistance, Color.green);

        if (hitCenterDetected || hitLeftDetected || hitRightDetected)
        {
            if (!isColliding && Time.time > _lastDetection + _timeBtwDetections)
            {
                Debug.Log("WAAALLLLLL");
                _mmfPlayer?.PlayFeedbacks();
                isColliding = true;
                _lastDetection = Time.time;


            }
            else
            {
                if (isColliding)
                {
                    Debug.Log("Sortie de la collision ");
                    isColliding = false;

                }
            }

        }
    }



















    // private void OnCollisionEnter(Collision col)
    // {
    //     if (col.gameObject.name == "CurveProto_1")
    //     {
    //         Debug.Log("COLLISION");
    //         // _mmfPlayer?.PlayFeedbacks();
    //     }
    // }
}
