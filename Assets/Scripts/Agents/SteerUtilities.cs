using System.Collections.Generic;
using UnityEngine;
using System;

public class MovementStatus {
	public Vector3 position;
	public Vector3 direction;
	public float linearSpeed;
	public float angularSpeed;
	public Collider[] neighbours;
	public int neighboursCount;
}

// To be extended by all movement behaviours
public abstract class SteeringBehaviour : MonoBehaviour {
	public abstract Vector3 GetAcceleration (MovementStatus status);
}

[Serializable] public class WeightedBehaviours : SerializableDictionary<SteeringBehaviour, float> { }

public class Blender {
	public static Vector3 Blend (List<Vector3> vl) {
		Vector3 result = Vector3.zero;
		foreach (Vector3 v in vl) result += v;
		return result;
	}
}

public class Driver {
	public static void DynamicSteer (Rigidbody body, MovementStatus status, Vector3 acceleration,
		                                float minV, float maxV, float maxSigma) {

		Vector3 tangentComponent = Vector3.Project (acceleration, status.direction);
		Vector3 normalComponent = acceleration - tangentComponent;

		float tangentAcc = tangentComponent.magnitude * Vector3.Dot (tangentComponent.normalized, status.direction);
		Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.direction.normalized;
		float rotationAcc = normalComponent.magnitude * Vector3.Dot (normalComponent.normalized, right) * 360f;

		float t = Time.deltaTime;

		float tangentDelta = status.linearSpeed * t + 0.5f * tangentAcc * t * t;
		float rotationDelta = status.angularSpeed * t + 0.5f * rotationAcc * t * t;

		status.linearSpeed += tangentAcc * t;
		status.angularSpeed += rotationAcc * t;

		status.linearSpeed = Mathf.Clamp (status.linearSpeed, minV, maxV);
		status.angularSpeed = Mathf.Clamp (status.angularSpeed, -maxSigma, maxSigma);

		if(tangentDelta > 0.01f)	// prevent sliding caused by numerical inaccuracy
			body.MovePosition (body.position + status.direction * tangentDelta);
		
		body.MoveRotation (body.rotation * Quaternion.Euler (0f, rotationDelta, 0f));
	}
}

