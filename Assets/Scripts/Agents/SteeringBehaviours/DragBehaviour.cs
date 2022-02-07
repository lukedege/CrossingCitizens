using UnityEngine;

public class DragBehaviour : SteeringBehaviour
{

	// how long does it take to stop moving by dragging
	public float linearDrag = 3f;
	public float angularDrag = 3f;

	private Vector3 dbgAcceleration;

	public override Vector3 GetAcceleration (MovementStatus status) {
		Vector3 acceleration = -(status.direction.normalized * status.linearSpeed / linearDrag)
								- ((Quaternion.Euler(0f, 90f, 0f) * status.direction.normalized) * status.angularSpeed / angularDrag);
		dbgAcceleration = acceleration;
		return acceleration;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + dbgAcceleration);
	}
}

