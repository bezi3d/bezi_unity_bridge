using UnityEngine;

namespace Bezel.Bridge.Editor.Settings
{
	public class BezelUnityBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezel Sync Key")]
        public string SyncKey = "";

        [Tooltip("The Unity Project Folder")]
        public string FileDirectory = "Assets/SampleFiles/";

        [Tooltip("Generate logic and linking of Bezel scene based on glTF's 'extras' information")]
        public bool BuildPrototypeFlow = false;
    }

}