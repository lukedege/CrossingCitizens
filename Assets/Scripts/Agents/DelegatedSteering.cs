using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class DelegatedSteering : MonoBehaviour {

	public float minLinearSpeed = 0.5f;
	public float maxLinearSpeed = 5f;
	public float maxAngularSpeed = 5f;
	public float fieldOfView = 2f;

	private MovementStatus status;

	public List<WeightedBehaviours> behaviourGroups = new List<WeightedBehaviours>();

	private void Start () {
		status = new MovementStatus ();
	}

	private void OnValidate()
	{
		LoadBehaviours();
	}

	public void LoadBehaviours()
	{
		if (behaviourGroups.Count == 0)
		{
			behaviourGroups.Add(new WeightedBehaviours());

			foreach (SteeringBehaviour bc in GetComponents<SteeringBehaviour>())
			{
				if (!behaviourGroups[0].ContainsKey(bc))
					behaviourGroups[0].Add(bc, 1f);
			}
		}
	}

	void FixedUpdate () 
	{
		// Filling up and refresh MovementStatus info
		status.direction = transform.forward;
		status.position = transform.position;
		status.neighboursCount = Physics.OverlapSphereNonAlloc(transform.position, fieldOfView, status.neighbours);

		// Scan all groups in order
		List<Vector3> components = new List<Vector3>();
		Vector3 blendedAcceleration = Vector3.zero;
		foreach (var bhvrGroup in behaviourGroups) 
		{
			// Blend the group
			foreach (var bhvrEntry in bhvrGroup)
			{
				components.Add(bhvrEntry.Key.GetAcceleration(status) * bhvrEntry.Value);
			}
			blendedAcceleration = Blender.Blend(components);
			// If the acceleration is more than epsilon than return, else
			// check next group
			if (blendedAcceleration.magnitude > 0.01f){	break; }
		}

		// if we have an acceleration, apply it
		Driver.Steer (GetComponent<Rigidbody> (), status, blendedAcceleration,
				          minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
	}

	private void OnDrawGizmos () {
		if (status != null) {
			UnityEditor.Handles.Label (transform.position + 2f * transform.up, status.linearSpeed.ToString () + "\n" + status.angularSpeed.ToString ());
		}
	}

}


