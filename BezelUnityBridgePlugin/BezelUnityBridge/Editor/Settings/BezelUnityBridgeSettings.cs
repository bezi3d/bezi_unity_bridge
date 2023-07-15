using UnityEngine;

namespace Bezel.Bridge.Editor.Settings
{
	public class BezelUnityBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezel Sync Key")]
        public string SyncKey = "";

        [Tooltip("The Unity Project Folder")]
        public string FileDirectory = "Assets/SampleFiles/";

        private string bezelFileUrl = "";

        public string getBezelFileURL() {
            return bezelFileUrl;
        }

        public void setBezelFileURL(string newValue)
        {
            bezelFileUrl = newValue;
        }
    }

}