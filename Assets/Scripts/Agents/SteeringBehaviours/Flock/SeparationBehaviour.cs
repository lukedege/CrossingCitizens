using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparationBehaviour : SteeringBehaviour
{
    public override Vector3 GetAcceleration(MovementStatus status)
    {
        return Vector3.zero;
    }
}
