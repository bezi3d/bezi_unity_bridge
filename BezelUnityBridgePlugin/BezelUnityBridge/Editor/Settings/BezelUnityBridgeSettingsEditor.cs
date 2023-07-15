using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bezel.Bridge.Editor.Settings;
using Codice.CM.Common.Tree;

[CustomEditor(typeof(BezelUnityBridgeSettings))]
public class BezelUnityBridgeSettingsEditor : Editor
{
    string inputText = "Hello";

    public override void OnInspectorGUI()
    {
        // Initialization
        BezelUnityBridgeSettings settings = target as BezelUnityBridgeSettings;
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 20;


        // Title
        GUILayout.Label("Bezel Bridge", titleStyle);
        //Rect titleRect = GUILayoutUtility.GetRect(100, 10);

        // Divider
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        // Just use the default editor
        DrawDefaultInspector();
        //EditorGUILayout.TextField("Name", inputText, EditorStyles.textField);

        // Divider
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Reference", EditorStyles.boldLabel);

        //EditorGUI.DrawRect(titleRect, Color.grey);


        if (GUILayout.Button("Link to Bezel file"))
        {
            Application.OpenURL(settings.getBezelFileURL());
        }




    }
}