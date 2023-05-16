using UnityEngine;
using UnityEditor;
using Bezel.Bridge.Editor.Settings;

namespace Bezel.Bridge.Editor
{
    public static class BezelUnityBridgeImporter
    {
        /// <summary>
        /// The settings asset, containing preferences for importing
        /// </summary>
        private static BezelBridgeSettings s_BezelBridgeSettings;

        [MenuItem("Bezel Bridge/Load from GLTF")]
        static void LoadFromGLTF()
        {
            Debug.Log("Bezel Editor: Load From GLTF");
        }

        [MenuItem("Bezel Bridge/Sync from Url")]
        static void SyncFromUrl()
        {
            Debug.Log("Bezel Editor: Sync From Url");
        }
    }
}