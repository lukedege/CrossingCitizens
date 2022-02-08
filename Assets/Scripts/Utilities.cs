using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utilities
{
    // Generate a random position at a certain "height" in a sphere of radius "range"
    public static Vector3 GenerateValidPosition(Vector3 position, float range, float height, Vector3 objectSize)
    {
        Vector3 randomPosition;
        int trial = -1;
        do
        {
            randomPosition = GenerateRandomPoint(position, range);
            randomPosition.y = height;
            trial++;
        } while (Physics.CheckSphere(randomPosition, MaxVector3Component(objectSize), LayerMask.GetMask("Citizen")) && trial < 3);

        return randomPosition;
    }

    // Generate a random position at a certain "height" in a capsule with a certain radius and a certain height along a certain direction
    public static Vector3 GenerateValidPositionCapsule(Vector3 position, float height, Vector3 objectSize, float capsRadius, float capsHeight, Vector3 direction)
    {
        Vector3 randomPosition, a, b;
        float alpha = UnityEngine.Random.value;
        int trial = -1;
        do
        {
            // Generate two random points in two spheres of capsRadius at capsHeight distance and lerp between these points
            a = GenerateRandomPoint(position - direction.normalized * capsHeight, capsRadius);
            b = GenerateRandomPoint(position + direction.normalized * capsHeight, capsRadius);
            a.y = b.y = height;
            randomPosition = Vector3.Lerp(a, b, alpha);
            trial++;
        } while (Physics.CheckSphere(randomPosition, MaxVector3Component(objectSize), LayerMask.GetMask("Citizen")) && trial < 3);

        return randomPosition;
    }

    public static float MaxVector3Component(Vector3 v)
    {
        return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
    }

    // Generate a random valid position (onto the NavMesh) given a point and a range 
    public static Vector3 GenerateValidPositionOnNavMesh(Vector3 position, float range)
    {
        return GenerateValidPositionOnNavMesh(position, range, null);
    }

    public static Vector3 GenerateValidPositionOnNavMesh(Vector3 position, float range, string areaName)
    {
        Vector3 randomDestination;
        Vector3 validatedDestination;
        do
        {
            randomDestination = GenerateRandomPoint(position, range);
            validatedDestination = OnNavMesh(randomDestination, range, areaName);
        } while (validatedDestination == null);

        return validatedDestination;
    }

    // Validate position onto a certain NavMesh area within a certain range
    public static Vector3 OnNavMesh(Vector3 position, float range, string areaName = null)
    {
        NavMeshHit hit;
        int navMeshAreaMask = areaName == null ? NavMesh.AllAreas : 1 << NavMesh.GetAreaFromName(areaName);
        NavMesh.SamplePosition(Flatten(position), out hit, range, navMeshAreaMask);
        return hit.position;
    }

    // Generate a truly random point in space given a point and a range
    public static Vector3 GenerateRandomPoint(Vector3 position, float range)
    {
        return position + UnityEngine.Random.insideUnitSphere * range;
    }

    // Flatten the Up component of a Vector3
    public static Vector3 Flatten(Vector3 v)
    {
        // equivalent to setting y component to 0;
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }

    // Checks whether the target vector is on the left side or the right side of the fwd vector
    //returns -1 when to the left, 1 to the right, and 0 for forward/backward
    public static float AngleDir(Vector3 forward, Vector3 target, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(forward, target);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            return -1.0f;
        }
        else
        {
            return 0.0f;
        }
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}