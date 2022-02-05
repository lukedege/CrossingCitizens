using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourSingle : SteeringBehaviour
{
	public float minSightRange = 0f;
	public float maxSightRange = 2f;
	public float sightDecay = 0.0075f;
	public float sightRegrowth = 0.005f;
	private float sightRange;

	public float steer = 15f;

	public float raycastVerticalOffset = 0f;

	private Vector3 dbgAcceleration;

    private void Awake()
    {
		sightRange = maxSightRange;
    }

    public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 acceleration = Vector3.zero;
		Vector3 verticalAdj = transform.position + Vector3.up * raycastVerticalOffset;
		bool hit; RaycastHit hitInfo;

		hit = Physics.Raycast (verticalAdj, status.direction, out hitInfo, sightRange);

		Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.direction.normalized; 

		if(hit)
        {
			sightRange -= sightDecay * Time.deltaTime;
			acceleration = right * steer * (1 / (hitInfo.distance + 0.001f));
		}
        else
        {
			sightRange += sightRegrowth * Time.deltaTime;
		}

		sightRange = Mathf.Clamp(sightRange, minSightRange, maxSightRange);

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

		Gizmos.DrawLine(verticalAdj, verticalAdj + transform.forward * sightRange);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(verticalAdj, verticalAdj + dbgAcceleration/2);
	}
}
