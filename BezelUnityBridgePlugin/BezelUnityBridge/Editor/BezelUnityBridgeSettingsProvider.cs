using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezel.Bridge.Editor
{
    public class BezelUnityBridgeSettingsProvider : SettingsProvider
    {
        private GUIStyle m_RedStyle;
        private GUIStyle m_GreenStyle;
        private BezelUnityBridgeSettings bezelUnityBridgeSettingsAsset;

        public BezelUnityBridgeSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
            m_RedStyle = new GUIStyle(EditorStyles.label);
            m_RedStyle.normal.textColor = UnityEngine.Color.red;

            m_GreenStyle = new GUIStyle(EditorStyles.label);
            m_GreenStyle.normal.textColor = UnityEngine.Color.green;
        }

        public static BezelUnityBridgeSettings FindUnityBridgeSettingsAsset()
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(BezelUnityBridgeSettings).Name}");
            if (assets == null || assets.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<BezelUnityBridgeSettings>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }

        public static bool IsSettingsAvailable()
        {
            return true;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            bezelUnityBridgeSettingsAsset = FindUnityBridgeSettingsAsset();
        }

        public override void OnGUI(string searchContext)
        {

            if (bezelUnityBridgeSettingsAsset == null)
            {
                GUILayout.Label("Create Bezel Unity Bridge Settings Asset");
                if (GUILayout.Button("Create..."))
                {
                    bezelUnityBridgeSettingsAsset = GenerateBezelUnityBridgeSettingsAsset();
                }

                return;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider =
                    new BezelUnityBridgeSettingsProvider("Project/Bezel Unity Bridge", SettingsScope.Project);
                return provider;
            }

            return null;
        }

        public static BezelUnityBridgeSettings GenerateBezelUnityBridgeSettingsAsset()
        {
            // Try create a new version asset.
            var newSettingsAsset = BezelUnityBridgeSettings.CreateInstance<BezelUnityBridgeSettings>();

            // Save to the project
            AssetDatabase.CreateAsset(newSettingsAsset, "Assets/BezelUnityBridgeSettings.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Generating BezelUnityBridgeSettings asset", newSettingsAsset);

            return newSettingsAsset;
        }
    }
}