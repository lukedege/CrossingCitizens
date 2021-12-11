using UnityEngine;

public class SeekBehaviour : SteeringBehaviour {
	public Transform target;

	public override Vector3 GetAcceleration (MovementStatus status) {
		return GetAcceleration(status, target);
	}

	// Static method to call when delegating to this behaviour
    public static Vector3 GetAcceleration(MovementStatus status, Transform target)
    {
		Vector3 verticalAdj = new Vector3(target.position.x, status.position.y, target.position.z);
		Vector3 toTarget = (verticalAdj - status.position);
		return toTarget;
    }
}

