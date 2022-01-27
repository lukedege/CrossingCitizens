using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourRVO : SteeringBehaviour
{

	public float sightRange = 5f;
	public float sightAngle = 45f;

	public float steer = 15f;
	public float backpedal = 10f;

	public float boxcastVerticalOffset = 0f;

	public bool leftWhisker = true;
	public bool midWhisker = true;
	public bool rightWhisker = true;

	public override Vector3 GetAcceleration (MovementStatus status) 
	{

		

		return Vector3.zero;
	}

	private void OnDrawGizmos()
	{

	}
}
