using UnityEngine;

public class ArriveBehaviour : SteeringBehaviour {
	public Transform target;
	public float gas = 3f;
	public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	public override Vector3 GetAcceleration (MovementStatus status) 
	{
		Vector3 verticalAdj = new Vector3(target.position.x, status.position.y, target.position.z);
		Vector3 toTarget = (verticalAdj - status.position);

		if (toTarget.magnitude > stopAt)
		{
			Vector3 tangentComponent = Vector3.Project(toTarget.normalized, status.direction);
			Vector3 normalComponent = (toTarget.normalized - tangentComponent);
			return (tangentComponent * (toTarget.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
		}
		else
		{
			return Vector3.zero;
		}
    }
}

