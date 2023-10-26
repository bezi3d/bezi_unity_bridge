//#define DEVMODE

using UnityEngine;
using UnityEditor;
using Bezi.Bridge.Editor;

[CustomEditor(typeof(BeziUnityBridgeSettings))]
public class BeziUnityBridgeSettingsEditor : Editor
{
    private Texture2D beziLogo;
    private string syncKey;

    string accessToken;

    void OnEnable() {
        // Load from Editor folder
        var assets = AssetDatabase.FindAssets("bezi_logo");
        if (assets == null || assets.Length == 0) return;
        beziLogo = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(Texture2D));
    }

    public override void OnInspectorGUI()
    {
        // Initialization
        BeziUnityBridgeSettings settings = target as BeziUnityBridgeSettings;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.fontSize = 20;

        GUIStyle disabledStyle = new GUIStyle(GUI.skin.button);
        disabledStyle.normal.textColor = Color.gray;
        string importButton = "Import Bezi file";
        string importAndShowButton = "Import Bezi file to Hierarchy";
        string constructBeziButton = "Bring Prefab to Hierarchy";

        // Title
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(beziLogo, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.LabelField("Bezi Bridge", titleStyle, GUILayout.Height(64), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        // Setting
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Step 1: Access Token", GUILayout.MaxWidth(130));
        EditorGUILayout.Space();
        accessToken = EditorGUILayout.PasswordField(
            EditorPrefs.GetString(BeziUnityBridgeImporter.BEZI_PERSONAL_ACCESS_TOKEN_PREF_KEY), 
            GUILayout.MaxWidth(300));
        EditorPrefs.SetString(BeziUnityBridgeImporter.BEZI_PERSONAL_ACCESS_TOKEN_PREF_KEY, accessToken);
        EditorGUILayout.EndHorizontal();
        
        settings.setFileLink(EditorGUILayout.TextField("Step 2: File Link", settings.getFileLink(), GUILayout.ExpandWidth(true)));

        settings.setFileDirectory(EditorGUILayout.TextField("Step 3: Unity File Directory", settings.getFileDirectory(), GUILayout.ExpandWidth(true)));

        EditorGUILayout.Separator();

#if DEVMODE
        // Setting
        EditorGUILayout.LabelField("Development Mode", EditorStyles.boldLabel);
        
        settings.setDevMode(EditorGUILayout.Toggle("Dev Mode", settings.getDevMode()));

        EditorGUILayout.Separator();

#endif

        // Setting
        EditorGUILayout.LabelField("Import Options", EditorStyles.boldLabel);

        settings.setShowInHierarchy(EditorGUILayout.Toggle("Show Prefab in Hierarchy", settings.getShowInHierarchy()));

        EditorGUILayout.Separator();

        // Import
        EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);

        string importButtonText = settings.getShowInHierarchy() ? importAndShowButton : importButton;
        if (GUILayout.Button(importButtonText))
        {
            BeziUnityBridgeImporter.ImportFromSyncKey();
        }

        if (BeziUnityBridgeImporter.importedGameObject)
        {
            if (GUILayout.Button(constructBeziButton))
            {
                BeziUnityBridgeImporter.BringPrefabIntoHierarchy();
            }
        }
        else
        {
            GUILayout.Button(constructBeziButton, disabledStyle);
        }

        if (BeziUnityBridgeImporter.importedGameObject == null)
        {
            return;
        }

        EditorGUILayout.LabelField("Go to folder "+ BeziUnityBridgeImporter.getBeziFolder() + " to drag the imported file into Hierarchy. ");


        // Reference
        if (settings.getBeziFileURL() == "") return;

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Reference", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Bezi File URL");
        GUILayout.Label(settings.getBeziFileURL());
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        if (GUILayout.Button("Open Bezi file on Browser"))
        {
            Application.OpenURL(settings.getBeziFileURL());
        }
    }
}