using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVO : SteeringBehaviour
{
	public float steer = 30f;

	private MovementStatus ownStatus;   // for debug gizmo
	private MovementStatus currentOtherStatus; // for debug gizmo
	private float angle;
	private Vector3 newV;

	public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 acceleration = Vector3.zero;

		// own data
		ownStatus = status;
		Vector3 ownPosition = status.position;
		Vector3 ownVelocity = status.linearSpeed * status.direction;
		float avoidRadius = transform.localScale.x; // radius of minkowski sum of B and -A aka (transform.localScale.x / 2) * 2

		// for each neighbour calculate their velocity obstacle
		for (int i = 0; i < status.neighboursCount; i++)
		{
			// retrieve neighbour data (position and velocity)
			Collider neighbour = status.neighbours[i];
			MovementStatus neighStatus = neighbour.gameObject.GetComponent<DelegatedSteering>().status;
			currentOtherStatus = neighStatus;
			Vector3 neighPosition = neighStatus.position;
			Vector3 neighVelocity = neighStatus.linearSpeed * neighStatus.direction;
			Vector3 toNeighbour = neighPosition - ownPosition;
			
			// calculate VO boundaries by finding the tangent angle (we assume both agents have the same dimension)
			// calculate the length of the line tangent by using Pythagora
			float tangentLineLength = Mathf.Sqrt(toNeighbour.magnitude * toNeighbour.magnitude - avoidRadius * avoidRadius);
			
			// calculate the angle of the tangent
			float boundaryAngle = Mathf.Atan2(avoidRadius, tangentLineLength) * Mathf.Rad2Deg;
			angle = boundaryAngle;

			// find the relative velocity 
			Vector3 relVel = ownVelocity - neighVelocity;

			// if the relative velocity vector is into the boundary angle, the agent will collide, thus needs to find another velocity
			bool inVO = Vector3.Dot(relVel.normalized, toNeighbour.normalized) >= Mathf.Cos(boundaryAngle);
			Vector3 newVel = Vector3.zero;

			// go è colpito dal raggio?
			if (inVO)
			{
				//find velocity outside boundary and return it
				float outsideBoundary = boundaryAngle + 10;
				newVel = Quaternion.Euler(0f, (Utilities.AngleDir(transform.forward, toNeighbour, transform.up) == 1 ? -outsideBoundary : outsideBoundary), 0f) * toNeighbour * steer;
			}

			// accumulate acceleration with the other neighbours
			acceleration += newVel;
		}
		//Debug.Log(acceleration);
		if(status.neighboursCount > 0)
			acceleration /= status.neighboursCount;
		newV = acceleration;
		return acceleration;
	}

	private void OnDrawGizmos()
	{
		if(ownStatus != null)
        {
			Gizmos.color = Color.green;
			Gizmos.DrawLine(ownStatus.position, ownStatus.position + ownStatus.linearSpeed * ownStatus.direction);
		}

		if (currentOtherStatus != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(ownStatus.position, currentOtherStatus.position);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(ownStatus.position, ownStatus.position + ownStatus.linearSpeed * ownStatus.direction - currentOtherStatus.linearSpeed * currentOtherStatus.direction);

			//Gizmos.color = Color.magenta;
			//Gizmos.DrawLine(ownStatus.position, ownStatus.position + Quaternion.Euler(0f, angle, 0f) * (currentOtherStatus.position - ownStatus.position));
			//Gizmos.DrawLine(ownStatus.position, ownStatus.position + Quaternion.Euler(0f, -angle, 0f) * (currentOtherStatus.position - ownStatus.position));
			//UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, transform.localScale.x);

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(ownStatus.position, ownStatus.position + ownStatus.linearSpeed * newV);
		}

	}
}
