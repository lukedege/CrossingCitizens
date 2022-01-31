using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVolume : SteeringBehaviour
{

	public float sightRange = 5f;
	public float sightAngle = 45f;

	public float steer = 15f;
	public float backpedal = 10f;

	public float boxcastVerticalOffset = 0f;

	public bool leftWhisker = true;
	public bool midWhisker = true;
	public bool rightWhisker = true;

	public override Vector3 GetAcceleration (MovementStatus status) {

		Collider collider = GetComponent<Collider> ();
		Vector3 verticalAdj = transform.position + Vector3.up * boxcastVerticalOffset;
		bool leftHit = false, midHit = false, rightHit = false;

		if (leftWhisker)
		{
			leftHit = Physics.BoxCast(verticalAdj,
											collider.bounds.extents,
											Quaternion.Euler(0f, -sightAngle, 0f) * status.direction,
											transform.rotation,
											sightRange, LayerMask.GetMask("Citizen", "Obstacles"));
		}

		if (midWhisker)
		{
			midHit = Physics.BoxCast(verticalAdj,
											  collider.bounds.extents,
											  status.direction,
											  transform.rotation,
											  sightRange, LayerMask.GetMask("Citizen", "Obstacles"));
		}

		if (rightWhisker)
		{
			rightHit = Physics.BoxCast(verticalAdj,
											 collider.bounds.extents,
											 Quaternion.Euler(0f, sightAngle, 0f) * status.direction,
											 transform.rotation,
											 sightRange, LayerMask.GetMask("Citizen", "Obstacles"));
		}

		Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.direction.normalized;

		if (leftHit && !midHit && !rightHit) {
			return right * steer;
		} else if (leftHit && midHit && !rightHit) {
			return right * steer * 2f;
		} else if (leftHit && midHit && rightHit) {
			return right * steer * 2f; //-status.direction.normalized * backpedal;
		} else if (!leftHit && midHit && rightHit) {
			return -right * steer * 2f;
		} else if (!leftHit && !midHit && rightHit) {
			return -right * steer;
		} else if (!leftHit && midHit && !rightHit) {
			return right * steer;
		}

		return Vector3.zero;
	}

	private void OnDrawGizmos()
	{
		Vector3 verticalAdj = transform.position + Vector3.up * boxcastVerticalOffset;
		Gizmos.color = new Color(0, 255, 0, 0.5f);

		if (leftWhisker)
		{
			Gizmos.DrawLine(verticalAdj, verticalAdj + Quaternion.Euler(0f, -sightAngle, 0f) * transform.forward * sightRange);
			Gizmos.DrawCube(verticalAdj + Quaternion.Euler(0f, -sightAngle, 0f) * transform.forward * sightRange, Vector3.one / 4);
		}
		if (midWhisker)
		{
			Gizmos.DrawCube(verticalAdj + transform.forward * sightRange, Vector3.one / 4);
			Gizmos.DrawLine(verticalAdj, verticalAdj + transform.forward * sightRange);
		}
		if (rightWhisker)
		{
			Gizmos.DrawCube(verticalAdj + Quaternion.Euler(0f, sightAngle, 0f) * transform.forward * sightRange, Vector3.one / 4);
			Gizmos.DrawLine(verticalAdj, verticalAdj + Quaternion.Euler(0f, sightAngle, 0f) * transform.forward * sightRange);
		}
	}
}
