using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVolumeAdaptive : SteeringBehaviour
{
	public float minSightRange = 0.3f;
	public float maxSightRange = 2f;
	public float sightDecay = 4f;
	public float sightRegrowth = 1.5f;
	private float sightRange;

	public float steer = 7f;

	public float boxcastVerticalOffset = 0f;
	public float boxcastForwardOffset = 0f;

	private Vector3 dbgAcceleration;

	private void Awake()
	{
		sightRange = maxSightRange;
	}

	public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 acceleration = Vector3.zero;
		Collider collider = GetComponent<Collider> ();
		Vector3 positionAdj = transform.position + Vector3.up * boxcastVerticalOffset + Vector3.forward * boxcastForwardOffset;
		bool hit = Physics.BoxCast(positionAdj,
											  collider.bounds.extents,
											  status.direction,
											  transform.rotation,
											  sightRange, LayerMask.GetMask("Citizen", "Obstacles"));

		Vector3 right = Quaternion.Euler(0f, 90f, 0f) * status.direction.normalized;

		if (hit)
		{
			acceleration = right * steer;

			sightRange -= sightDecay * Time.deltaTime;
		}
		else
		{
			sightRange += sightRegrowth * Time.deltaTime;
		}

		sightRange = Mathf.Clamp(sightRange, minSightRange, maxSightRange);

		dbgAcceleration = acceleration;

		return acceleration;
	}

	private void OnDrawGizmos()
	{
		Vector3 verticalAdj = transform.position + Vector3.up * boxcastVerticalOffset;
		Gizmos.color = new Color(0, 255, 0, 0.5f);

		Gizmos.DrawCube(verticalAdj + transform.forward * sightRange, Vector3.one / 4);
		Gizmos.DrawLine(verticalAdj, verticalAdj + transform.forward * sightRange);
		
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(verticalAdj, verticalAdj + dbgAcceleration);
	}
}
