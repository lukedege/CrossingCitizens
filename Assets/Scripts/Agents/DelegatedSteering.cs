using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]

public class DelegatedSteering : MonoBehaviour {

	public float minLinearSpeed = 0.5f;
	public float maxLinearSpeed = 5f;
	public float maxAngularSpeed = 5f;

	private MovementStatus status;
	public Dictionary<SteeringBehaviour, float> weightedBehaviours = new Dictionary<SteeringBehaviour, float>();

	private void Start () {
		status = new MovementStatus ();
	}

	private void OnValidate()
	{
		LoadBehaviours();
	}

	public void LoadBehaviours()
	{
		foreach (SteeringBehaviour bc in GetComponents<SteeringBehaviour>())
		{
			if (!weightedBehaviours.ContainsKey(bc))
				weightedBehaviours.Add(bc, 1f);
		}
	}

	void FixedUpdate () 
	{
		// Filling up and refresh MovementStatus info
		status.direction = transform.forward;
		status.position = transform.position;

		// Contact all behaviours and build a list of directions
		List<Vector3> components = new List<Vector3> ();
		foreach (SteeringBehaviour bhvr in weightedBehaviours.Keys)
			components.Add (bhvr.GetAcceleration(status) * weightedBehaviours[bhvr]);

		// Blend the list to obtain a single acceleration to apply
		Vector3 blendedAcceleration = Blender.Blend (components);

		// if we have an acceleration, apply it
		if (blendedAcceleration.magnitude > 0.01f) {
			Driver.Steer (GetComponent<Rigidbody> (), status, blendedAcceleration,
				          minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
		}
	}

	private void OnDrawGizmos () {
		if (status != null) {
			UnityEditor.Handles.Label (transform.position + 2f * transform.up, status.linearSpeed.ToString () + "\n" + status.angularSpeed.ToString ());
		}
	}

}
