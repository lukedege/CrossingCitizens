using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrioritySteering))]
public class PrioritySteeringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        PrioritySteering agent = (PrioritySteering)target;

        EditorGUILayout.Separator();
        DrawUILine(Color.grey);

        // Behaviour weights label
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Steering behaviour weights", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset"))
        {
            agent.behaviourGroups.Clear();
            agent.LoadBehaviours();
        }

        EditorGUILayout.EndHorizontal();

        // Update groups
        var oldBhvrGroups = new List<Dictionary<SteeringBehaviour, float>>(); // saving the old one for display while calculating the new values for the next iteration
        foreach (var bhvrGroup in agent.behaviourGroups)
        {
            oldBhvrGroups.Add(new Dictionary<SteeringBehaviour, float>(bhvrGroup));
        }

        int currGroup = 0, newGroup = 0;
        foreach (var bhvrGroup in oldBhvrGroups)
        {
            if (bhvrGroup.Count == 0)
            {
                agent.behaviourGroups.RemoveAt(currGroup);
            }
            else
            {
                EditorGUILayout.LabelField("Group " + currGroup + ":", EditorStyles.boldLabel);
                foreach (var bhvr in bhvrGroup)
                {
                    EditorGUILayout.BeginHorizontal();

                    agent.behaviourGroups[currGroup][bhvr.Key] = EditorGUILayout.Slider(bhvr.Key.GetType().Name, bhvr.Value, 0, 1f);

                    EditorGUILayout.LabelField("Group", GUILayout.Width(50));

                    newGroup = EditorGUILayout.IntField(currGroup, GUILayout.Width(20));
                    if (newGroup != currGroup)
                    {
                        if (newGroup >= agent.behaviourGroups.Count)
                        {
                            newGroup = agent.behaviourGroups.Count;
                            agent.behaviourGroups.Add(new WeightedBehaviours());
                        }
                        agent.behaviourGroups[newGroup].Add(bhvr.Key, bhvr.Value);
                        agent.behaviourGroups[currGroup].Remove(bhvr.Key);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                currGroup++;
            }

        }

    }

    public static void DrawUILine(Color color, int thickness = 1, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        EditorGUI.DrawRect(r, color);
    }
}
