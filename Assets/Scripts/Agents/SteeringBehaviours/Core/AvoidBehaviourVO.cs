using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVO : SteeringBehaviour
{

	public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 acceleration = Vector3.zero;

		// own data
		Vector3 ownPosition = status.position;
		Vector3 ownVelocity = status.linearSpeed * status.direction;
		Vector3 agentSize = Quaternion.Euler(0f, 90f, 0f) * transform.localScale;

		// for each neighbour calculate their velocity obstacle
		foreach (Collider neighbour in status.neighbours)
		{
			// retrieve neighbour data (position and velocity)
			MovementStatus neighStatus = neighbour.gameObject.GetComponent<DelegatedSteering>().status;
			Vector3 neighPosition = neighStatus.position;
			Vector3 neighVelocity = neighStatus.linearSpeed * neighStatus.direction;
			Vector3 toNeighbour = neighPosition - ownPosition;

			// calculate VO boundaries (we assume both agents have the same dimension)
			Vector3 boundaryLeft = 2 * (toNeighbour) + (agentSize / 2); // TODO approssimazione erronea, serve la tangente
			Vector3 boundaryRight = 2 * (toNeighbour - ownPosition) - (agentSize / 2); // TODO approssimazione erronea, serve la tangente

			float boundaryAngle = 0; // find tangent angle between boundaries (BETTER SOLUTION THAN TANGENTS)

			// find the relative velocity 
			Vector3 relVel = ownVelocity - neighVelocity;

			// if the relative velocity vector is between the boundaries, the agent will collide, thus needs to find another velocity
			bool inVO = Vector3.Dot(relVel, toNeighbour) >= boundaryAngle;
			Vector3 newVel = Vector3.zero;

			// go è colpito dal raggio?
			if (inVO)
			{
				//find velocity outside boundary and return it
			}

			// accumulate acceleration with the other neighbours
			acceleration += newVel;
		}

		return acceleration;
	}

	private void OnDrawGizmos()
	{

	}
}
