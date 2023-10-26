using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezi.Bridge.Editor
{
    public class BeziUnityBridgeSettingsProvider : SettingsProvider
    {
        private GUIStyle m_RedStyle;
        private GUIStyle m_GreenStyle;
        private BeziUnityBridgeSettings beziUnityBridgeSettingsAsset;

        public BeziUnityBridgeSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
            m_RedStyle = new GUIStyle(EditorStyles.label);
            m_RedStyle.normal.textColor = UnityEngine.Color.red;

            m_GreenStyle = new GUIStyle(EditorStyles.label);
            m_GreenStyle.normal.textColor = UnityEngine.Color.green;
        }

        public static BeziUnityBridgeSettings FindUnityBridgeSettingsAsset()
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(BeziUnityBridgeSettings).Name}");
            if (assets == null || assets.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<BeziUnityBridgeSettings>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }

        public static bool IsSettingsAvailable()
        {
            return true;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            beziUnityBridgeSettingsAsset = FindUnityBridgeSettingsAsset();
        }

        public override void OnGUI(string searchContext)
        {

            if (beziUnityBridgeSettingsAsset == null)
            {
                GUILayout.Label("Create Bezi Unity Bridge Settings Asset");
                if (GUILayout.Button("Create..."))
                {
                    beziUnityBridgeSettingsAsset = GenerateBeziUnityBridgeSettingsAsset();
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
                    new BeziUnityBridgeSettingsProvider("Project/Bezi Unity Bridge", SettingsScope.Project);
                return provider;
            }

            return null;
        }

        public static BeziUnityBridgeSettings GenerateBeziUnityBridgeSettingsAsset()
        {
            // Try create a new version asset.
            var newSettingsAsset = BeziUnityBridgeSettings.CreateInstance<BeziUnityBridgeSettings>();

            // Save to the project
            AssetDatabase.CreateAsset(newSettingsAsset, "Assets/BeziUnityBridgeSettings.asset");
            AssetDatabase.SaveAssets();

            return newSettingsAsset;
        }
    }
}