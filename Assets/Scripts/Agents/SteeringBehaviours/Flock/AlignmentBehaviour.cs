using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehaviour : SteeringBehaviour
{
    public override Vector3 GetAcceleration(MovementStatus status)
    {
        Vector3 alignment = Vector3.zero;

        // calculate the average of all the neighbourhood
        // elements direction
        for (int i = 0; i < status.neighboursCount; i++)
        {
            // if the neighbour is a boid and of the same flock
            if (status.neighbours[i].gameObject.layer == gameObject.layer)
                alignment += status.neighbours[i].gameObject.transform.forward;
        }

        // normalize the alignment direction 
        alignment = alignment.normalized;

        return alignment;
    }
}

