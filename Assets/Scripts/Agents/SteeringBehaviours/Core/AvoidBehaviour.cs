using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviour : SteeringBehaviour
{
	public float sightRange = 5f;
	public float sightAngle = 45f;

	public float steer = 15f;
	public float backpedal = 10f;

	public float raycastVerticalOffset = 0f;

	public bool leftWhisker = true;
	public bool midWhisker = true;
	public bool rightWhisker = true;

	private Vector3 dbgAcceleration;

	public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 acceleration = Vector3.zero;
		Vector3 verticalAdj = transform.position + Vector3.up * raycastVerticalOffset;
		bool leftHit = false, midHit = false, rightHit = false;
		float lateralSightRange = sightRange;

		if(leftWhisker)
			leftHit = Physics.Raycast (verticalAdj, Quaternion.Euler (0f, -sightAngle, 0f) * status.direction, sightRange);
		if(midWhisker)
			midHit = Physics.Raycast (verticalAdj, status.direction, sightRange);
		if(rightWhisker)
			rightHit = Physics.Raycast (verticalAdj, Quaternion.Euler (0f, sightAngle, 0f) * status.direction, sightRange);

		Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.direction.normalized;

		if (leftHit && !midHit && !rightHit) {
			acceleration = right * steer;
		} else if (leftHit && midHit && !rightHit) {
			acceleration = right * steer * 2f;
		} else if (leftHit && midHit && rightHit) {
			acceleration = -status.direction.normalized * backpedal;
		} else if (!leftHit && midHit && rightHit) {
			acceleration = -right * steer * 2f;
		} else if (!leftHit && !midHit && rightHit) {
			acceleration = -right * steer;
		} else if (!leftHit && midHit && !rightHit) {
			acceleration = right * steer;
		}

		dbgAcceleration = acceleration;

		return acceleration;
	}


	private Vector3 ObjectSize (GameObject go) {
		Bounds b = new Bounds (go.transform.position, Vector3.zero);
		foreach (Collider c in go.GetComponentsInChildren<Collider> ()) {
			b.Encapsulate (c.bounds);
		}
		return b.size;
	}

	private void OnDrawGizmos()
	{
		Vector3 verticalAdj = transform.position + Vector3.up * raycastVerticalOffset;
		Gizmos.color = new Color(0, 255, 0, 0.5f);

		if (leftWhisker)
			Gizmos.DrawLine(verticalAdj, verticalAdj + Quaternion.Euler(0f, -sightAngle, 0f) * transform.forward * sightRange);
		if (midWhisker)
			Gizmos.DrawLine(verticalAdj, verticalAdj + transform.forward * sightRange);
		if (rightWhisker)
			Gizmos.DrawLine(verticalAdj, verticalAdj + Quaternion.Euler(0f, sightAngle, 0f) * transform.forward * sightRange);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(verticalAdj, verticalAdj + dbgAcceleration);
	}
}
