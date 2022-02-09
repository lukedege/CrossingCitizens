using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Behaviour that describes a spawner of crossing citizens which will pursue a certain assigned destination
public class CrossingCitizenSpawner : MonoBehaviour
{
    [Header("Spawn properties")]
    public GameObject citizenTemplate;          // The GameObject template of the citizen to spawn
    public int amount = 10;                     // The amount of instances of the template to spawn
    public float lengthAlongSidewalk = 14f;     // The size of the spawn area along the spawner's right vector (aka the length along the sidewalk)
    public float lengthAcrossSidewalk = 6f;     // The size of the spawn area along the spawner's forward vector (aka the length across the sidewalk)
    public Transform baseDestination;          // The base destination the citizen should reach

    void Start()
    {
        if (IsTemplateValid())
        {
            for (int i = 0; i < amount; i++)
            {
                // Set initial position and rotation
                Vector3 spawnPosition = Utilities.GenerateValidPositionCapsule(transform.position, citizenTemplate.transform.localScale.y,
                    lengthAcrossSidewalk * 0.5f, lengthAlongSidewalk, transform.right);
                Quaternion spawnRotation = transform.rotation;

                // Generate instance
                GameObject citizenInstance = Instantiate(citizenTemplate, spawnPosition, spawnRotation);
                citizenInstance.GetComponent<Crosser>().baseDestination = baseDestination;
            }
        }
        else
        {
            Debug.LogError("Provided template has no Crosser component!");
        }
    }

    // Checks if provided template has the needed Crosser component
    bool IsTemplateValid()
    {
        return citizenTemplate.GetComponent<Crosser>() != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position + lengthAlongSidewalk * transform.right, transform.up, lengthAcrossSidewalk * 0.5f);
        UnityEditor.Handles.DrawWireDisc(transform.position - lengthAlongSidewalk * transform.right, transform.up, lengthAcrossSidewalk * 0.5f);
        Gizmos.DrawLine(transform.position - lengthAlongSidewalk * transform.right + lengthAcrossSidewalk * transform.forward * 0.5f, transform.position + lengthAlongSidewalk * transform.right + lengthAcrossSidewalk * transform.forward * 0.5f);
        Gizmos.DrawLine(transform.position - lengthAlongSidewalk * transform.right - lengthAcrossSidewalk * transform.forward * 0.5f, transform.position + lengthAlongSidewalk * transform.right - lengthAcrossSidewalk * transform.forward * 0.5f);
    }
}
