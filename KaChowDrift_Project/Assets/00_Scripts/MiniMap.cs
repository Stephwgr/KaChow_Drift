using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{

    [SerializeField] private Transform _player;
    [SerializeField] private float _offsetCamMinimap;
    

    
    void Update()
    {
        Vector3 newPosition = _player.position;
        newPosition.y = transform.position.y;
        newPosition.z += _offsetCamMinimap;
        transform.position = newPosition;
    }
}
