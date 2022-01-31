using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RVOSteering : MonoBehaviour
{
    public Transform destination;

    public float gas = 3f;
    public float steer = 30f;
    public float brake = 20f;

    public float brakeAt = 5f;
    public float stopAt = 0.01f;

    private Vector3 preferredVelocity;

    void Start()
    {
        
    }

    void Update()
    {
        preferredVelocity = destination.position - transform.position;
    }
}
