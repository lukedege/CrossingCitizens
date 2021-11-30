using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenSpawner : MonoBehaviour
{
    [Header("Spawn properties")]
    public GameObject template;
    public int amount = 50;
    public float range = 50;

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            // Set initial position and rotation
            Vector3 spawnPosition = Utilities.GenerateValidPosition(transform.position, range);
            spawnPosition.y = template.transform.localScale.y; // Flattening
            Vector3 spawnRotation = new Vector3(0, UnityEngine.Random.value, 0) * 360;

            // Generate instance
            GameObject boidInstance = Instantiate(template, spawnPosition, Quaternion.Euler(spawnRotation));
        }
    }
}
