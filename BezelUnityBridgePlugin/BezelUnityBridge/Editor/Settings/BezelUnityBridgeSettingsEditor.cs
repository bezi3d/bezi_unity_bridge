//#define DEVMODE

using UnityEngine;
using UnityEditor;
using Bezel.Bridge.Editor.Settings;
using UnityEditor.Callbacks;

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
        string constructTextButton = "Optimize Text";

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
        // Import
        EditorGUILayout.LabelField("Import and Optimize", EditorStyles.boldLabel);

        if (GUILayout.Button("Import Bezel file"))
        {
            BezelUnityBridgeImporter.ImportFromSyncKey();
        }

        if (BezelUnityBridgeImporter.importedGameObject)
        {
            if (GUILayout.Button(constructTextButton))
            {
                BezelUnityBridgeImporter.ConstructText();
            }
        }
        else {
            GUILayout.Button(constructTextButton, disabledStyle);
        }

        EditorGUILayout.Separator();

        // Reference
        if(settings.getBezelFileURL() == "") return;

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