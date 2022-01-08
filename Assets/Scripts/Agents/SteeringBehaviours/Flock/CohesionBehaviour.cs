using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CohesionBehaviour : SteeringBehaviour
{
    public override Vector3 GetAcceleration(MovementStatus status)
    {
        Vector3 cohesion = Vector3.zero;
        Vector3 baricenter = Vector3.zero;
        float counter = 0f;

        // calculate the average of all the neighbourhood
        // elements to find the baricenter of the flock
        for (int i = 0; i < status.neighboursCount; i++)
        {
            if (status.neighbours[i].gameObject.layer == gameObject.layer)
            {
                baricenter += status.neighbours[i].transform.position;
                counter++;
            }
        }

        baricenter /= counter;

        // find the vector from me to the baricenter
        cohesion = baricenter - transform.position;

        // normalize and weigh
        cohesion = cohesion.normalized;

        return cohesion;
    }
}

