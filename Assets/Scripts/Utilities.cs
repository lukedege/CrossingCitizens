using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utilities
{
    // Generate a random position at a certain "height" in a sphere of radius "range"
    public static Vector3 GenerateValidPosition(Vector3 position, float range, float height)
    {
        Vector3 randomPosition = GenerateRandomPoint(position, range);
        randomPosition.y = height;

        return randomPosition;
    }

    // Generate a random position at a certain "height" in a capsule with a certain radius and a certain height along a certain direction
    public static Vector3 GenerateValidPositionCapsule(Vector3 position, float height, float capsRadius, float capsHeight, Vector3 direction)
    {
        Vector3 randomPosition, a, b;
        float alpha = UnityEngine.Random.value;

        // Generate two random points in two spheres of capsRadius at capsHeight distance and lerp between these points
        a = GenerateRandomPoint(position - direction.normalized * capsHeight, capsRadius);
        b = GenerateRandomPoint(position + direction.normalized * capsHeight, capsRadius);
        a.y = b.y = height;
        randomPosition = Vector3.Lerp(a, b, alpha);

        return randomPosition;
    }

    public static float MaxVector3Component(Vector3 v)
    {
        return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
    }

    // Generate a truly random point in space given a point and a range
    public static Vector3 GenerateRandomPoint(Vector3 position, float range)
    {
        return position + UnityEngine.Random.insideUnitSphere * range;
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

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}