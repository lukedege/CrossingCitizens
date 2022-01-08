using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utilities
{
    // Generate a random valid position (aka onto the NavMesh) given a point and a range 
    public static Vector3 GenerateValidPosition(Vector3 position, float range)
    {
        return GenerateValidPosition(position, range, null);
    }

    public static Vector3 GenerateValidPosition(Vector3 position, float range, string areaName)
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