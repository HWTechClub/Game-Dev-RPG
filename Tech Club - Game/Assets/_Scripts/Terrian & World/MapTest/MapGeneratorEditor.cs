﻿#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        DrawDefaultInspector();

        //if (mapGen.autoUpdate)
        //    mapGen.GenerateMap();


        if (GUILayout.Button ("Generate")) {
            mapGen.GenerateMap();
        }
    }
}
#endif
