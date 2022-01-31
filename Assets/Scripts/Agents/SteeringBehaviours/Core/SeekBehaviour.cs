﻿using UnityEngine;

public class SeekBehaviour : SteeringBehaviour 
{

	public Vector3 destination;

	public float gas = 3f;
	public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	private Vector3 dbgAcceleration;

	public override Vector3 GetAcceleration (MovementStatus status) {
		Vector3 acceleration = Vector3.zero;
		if (destination != null)
		{
			Vector3 verticalAdj = new Vector3(destination.x, transform.position.y, destination.z);
			Vector3 toDestination = (verticalAdj - transform.position);

			if (toDestination.magnitude > stopAt)
			{
				Vector3 tangentComponent = Vector3.Project(toDestination.normalized, status.direction);
				Vector3 normalComponent = (toDestination.normalized - tangentComponent);
				acceleration = (toDestination.normalized * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
			}
		}
		dbgAcceleration = acceleration;
		return acceleration;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + dbgAcceleration);
	}
}

