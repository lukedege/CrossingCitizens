using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DelegatedSteering))]
public class DelegatedSteeringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        DelegatedSteering agent = (DelegatedSteering)target;

        EditorGUILayout.Separator();
        DrawUILine(Color.grey);

        // Behaviour weights label
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Steering behaviour weights", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh"))
        {
            agent.weightedBehaviours.Clear();
            agent.LoadBehaviours();
        }

        EditorGUILayout.EndHorizontal();

        // Update weighs from slider values
        var oldBhvrWeights = new Dictionary<SteeringBehaviour, float>(agent.weightedBehaviours); 

        foreach (var bhvr in oldBhvrWeights)
        {
            agent.weightedBehaviours[bhvr.Key] = EditorGUILayout.Slider(bhvr.Key.GetType().Name, bhvr.Value, 0, 1f);
        }
    }

    public static void DrawUILine(Color color, int thickness = 1, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        EditorGUI.DrawRect(r, color);
    }
}
