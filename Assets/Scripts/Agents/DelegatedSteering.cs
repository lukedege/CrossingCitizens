using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class DelegatedSteering : MonoBehaviour {

	public float minLinearSpeed = 0.5f;
	public float maxLinearSpeed = 5f;
	public float maxAngularSpeed = 5f;
	public float fieldOfView = 2f;

	public bool showDebugGizmos = true;

	public MovementStatus status;

	public List<WeightedBehaviours> behaviourGroups = new List<WeightedBehaviours>();

	// debug info
	private string currBhvrGroup;
	private string currBhvr;

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
		status.neighbours = new Collider[200];

		// disable myself to avoid flocking from myself :)
		GetComponent<Collider>().enabled = false;
		status.neighboursCount = Physics.OverlapSphereNonAlloc(transform.position, fieldOfView, status.neighbours, LayerMask.GetMask("Citizen"));
		GetComponent<Collider>().enabled = true;

		// Scan all groups in order
		List<Vector3> components = new List<Vector3>();
		Vector3 blendedAcceleration = Vector3.zero;
		int i = 0; // DEBUG INFO
		foreach (var bhvrGroup in behaviourGroups) 
		{
			currBhvrGroup = i+"";
			i++;
			// Blend the group
			foreach (var bhvrEntry in bhvrGroup)
			{
				components.Add(bhvrEntry.Key.GetAcceleration(status) * bhvrEntry.Value);
			}
			blendedAcceleration = Blender.Blend(components);
			// If the acceleration is more than epsilon than return, else
			// check next group
			if (blendedAcceleration.magnitude > 0.001f){ break; }
		}

		// if we have an acceleration, apply it
		if (blendedAcceleration.magnitude > 0.001f)
		{
			Driver.DynamicSteer (GetComponent<Rigidbody> (), status, blendedAcceleration, minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
			//Driver.KinematicSteer(transform, status, blendedAcceleration, minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
		}
	}

	private void OnDrawGizmos () {
		if (status != null) {
			UnityEditor.Handles.Label (transform.position + 2f * transform.up, status.linearSpeed.ToString () + "\n" + status.angularSpeed.ToString ()
				+ "\n" + currBhvrGroup);
			UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, fieldOfView);
		}
	}

}


