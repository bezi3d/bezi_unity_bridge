using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bezel.Bridge.Editor.Settings;
using Codice.CM.Common.Tree;

[CustomEditor(typeof(BezelUnityBridgeSettings))]
public class BezelUnityBridgeSettingsEditor : Editor
{
    private Texture2D bezelLogo;

    void OnEnable() {
        // Load from Editor folder
        var assets = AssetDatabase.FindAssets("bezel_icon_logo");
        if (assets == null || assets.Length == 0) return;
        bezelLogo = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Texture2D));
    }

    public override void OnInspectorGUI()
    {
        // Initialization
        BezelUnityBridgeSettings settings = target as BezelUnityBridgeSettings;
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 20;
        
        // Title
        GUILayout.BeginHorizontal();
        GUILayout.Label(bezelLogo, GUILayout.Width(64), GUILayout.Height(64));
        GUILayout.Label("Bezel Bridge", titleStyle, GUILayout.Height(64), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        // Divider
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        // Just use the default editor
        DrawDefaultInspector();
        //EditorGUILayout.TextField("Name", inputText, EditorStyles.textField);

        // Divider
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);


        if (GUILayout.Button("Import Bezel file into Unity"))
        {
            BezelUnityBridgeImporter.ImportFromSyncKey();
        }

        // Divider
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Reference", EditorStyles.boldLabel);

        //EditorGUI.DrawRect(titleRect, Color.grey);


        if (GUILayout.Button("Open Bezel file on Browser"))
        {
            Application.OpenURL(settings.getBezelFileURL());
        }
    }
}