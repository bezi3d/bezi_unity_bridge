//#define DEVMODE

using UnityEngine;
using UnityEditor;
using Bezel.Bridge.Editor;

[CustomEditor(typeof(BezelUnityBridgeSettings))]
public class BezelUnityBridgeSettingsEditor : Editor
{
    private Texture2D bezelLogo;
    private string syncKey;

    string accessToken;

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

        GUIStyle disabledStyle = new GUIStyle(GUI.skin.button);
        disabledStyle.normal.textColor = Color.gray;
        string importButton = "Import Bezel file";
        string importAndShowButton = "Import Bezel file to Hierarchy";
        string constructBezelButton = "Bring Prefab to Hierarchy";

        // Title
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(bezelLogo, GUILayout.Width(64), GUILayout.Height(64));
        EditorGUILayout.LabelField("Bezel Bridge", titleStyle, GUILayout.Height(64), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        // Setting
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Step 1: Access Token", GUILayout.MaxWidth(130));
        EditorGUILayout.Space();
        accessToken = EditorGUILayout.PasswordField(
            EditorPrefs.GetString(BezelUnityBridgeImporter.BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY), 
            GUILayout.MaxWidth(300));
        EditorPrefs.SetString(BezelUnityBridgeImporter.BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY, accessToken);
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
            BezelUnityBridgeImporter.ImportFromSyncKey();
        }

        if (BezelUnityBridgeImporter.importedGameObject)
        {
            if (GUILayout.Button(constructBezelButton))
            {
                BezelUnityBridgeImporter.BringPrefabIntoHierarchy();
            }
        }
        else
        {
            GUILayout.Button(constructBezelButton, disabledStyle);
        }

        if (BezelUnityBridgeImporter.importedGameObject == null)
        {
            return;
        }

        EditorGUILayout.LabelField("Go to folder "+ BezelUnityBridgeImporter.getBezelFolder() + " to drag the imported file into Hierarchy. ");


        // Reference
        if (settings.getBezelFileURL() == "") return;

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Reference", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Bezel File URL");
        GUILayout.Label(settings.getBezelFileURL());
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        if (GUILayout.Button("Open Bezel file on Browser"))
        {
            Application.OpenURL(settings.getBezelFileURL());
        }
    }
}