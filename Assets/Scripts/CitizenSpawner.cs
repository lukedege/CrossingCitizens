using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CitizenSpawner : MonoBehaviour
{
    [Header("Spawn properties")]
    public GameObject template;
    public int amount = 50;
    public float range = 50;
    public GameObject destination; //TEMPORARY

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            // Set initial position and rotation
            Vector3 spawnPosition = Utilities.GenerateValidPosition(transform.position, range, template.transform.localScale.y, template.transform.localScale);
            Quaternion spawnRotation = transform.rotation;

            // Generate instance

            GameObject boidInstance = Instantiate(template, spawnPosition, spawnRotation);
            boidInstance.GetComponent<SeekBehaviour>().destination = destination.transform;
            //boidInstance.GetComponent<NavMeshAgent>().destination = destination.transform.position;
        }
    }
}
