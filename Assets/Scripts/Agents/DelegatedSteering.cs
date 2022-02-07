using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class DelegatedSteering : MonoBehaviour 
{
	public float minLinearSpeed = 0.5f;
	public float maxLinearSpeed = 5f;
	public float maxAngularSpeed = 5f;
	public float fieldOfView = 2f;

	[HideInInspector]
	public MovementStatus status;
	[HideInInspector]
	public List<WeightedBehaviours> behaviourGroups = new List<WeightedBehaviours>();

	// debug info
	private string currBhvrGroup;

	private void Start () {
		status = new MovementStatus ();

		status.position = transform.position;
		status.direction = transform.forward;
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

	public void ChangeBehaviourWeight<T>(float newWeight) where T : SteeringBehaviour
    {
		newWeight = Mathf.Clamp01(newWeight);
		SteeringBehaviour bhvrToChange = null;
		foreach(var bhvrGroup in behaviourGroups)
        {
			foreach(var bhvr in bhvrGroup)
            {
				if (bhvr.Key.GetType() == typeof(T))
					bhvrToChange = bhvr.Key;
            }
			if (bhvrToChange != null)
			{
				bhvrGroup[bhvrToChange] = newWeight;
				break;
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
		status.neighboursCount = Physics.OverlapSphereNonAlloc(transform.position + transform.forward*1f, fieldOfView, status.neighbours, LayerMask.GetMask("Citizen","Obstacles"));
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
		if (blendedAcceleration.magnitude > 0.1f)
		{
			Driver.DynamicSteer (GetComponent<Rigidbody> (), status, blendedAcceleration, minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
			//Driver.KinematicSteer(transform, status, blendedAcceleration, minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
		}
	}

	private void OnDrawGizmos () {
		if (status != null) {
			UnityEditor.Handles.Label (transform.position + 2f * transform.up, status.linearSpeed.ToString () + "\n" + status.angularSpeed.ToString ()
				+ "\n" + currBhvrGroup);
			UnityEditor.Handles.DrawWireDisc(transform.position + transform.forward*1f, transform.up, fieldOfView);
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(status.position, status.position + status.linearSpeed * status.direction * 3);
		}
	}

}


