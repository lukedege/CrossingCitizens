using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparationBehaviour : SteeringBehaviour
{
    public override Vector3 GetAcceleration(MovementStatus status)
    {
        Vector3 separation = Vector3.zero;
        Vector3 distanceFrom = Vector3.zero;

        // we check EVERY neighbour because we want to stay away from other boids but stay on the floor
        for (int i = 0; i < status.neighboursCount; i++)
        {
            if (status.neighbours[i].gameObject.layer != LayerMask.NameToLayer("Floor"))
            {
                // we calculate the distance vector between us and the neighbour,
                // looking away from the closest point on its bounds
                distanceFrom = transform.position - status.neighbours[i].ClosestPointOnBounds(transform.position);

                // and now we calculate the repulsion force in this direction
                // (the closer we are, the higher the repulsion force)
                separation += distanceFrom.normalized / (distanceFrom.magnitude + 0.0001f);
            }

        }
        // normalize and weigh
        separation = separation.normalized;

        return separation;
    }
}
