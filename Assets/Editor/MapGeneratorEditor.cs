using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("GenerateMap"))
        {
            ((MapGenerator)target).GenerateMap();
        }

        if (GUILayout.Button("CleanMap"))
        {
            ((MapGenerator)target).CleanMap();
        }
    }
}