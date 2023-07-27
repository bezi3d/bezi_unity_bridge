using UnityEngine;

namespace Bezel.Bridge.Editor.Settings
{
	public class BezelUnityBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezel Sync Key")]
        public string SyncKey = "";

        [Tooltip("The Unity Project Folder")]
        public string FileDirectory = "Assets/BezelFiles/";

        public string BezelFileURL = "";

        public string getSyncKey() {
            return SyncKey;
        }

        public void setSyncKey(string newValue)
        {
            SyncKey = newValue;
        }

        public string getFileDirectory() {
            return FileDirectory;
        }

        public void setFileDirectory(string newValue)
        {
            FileDirectory = newValue;
        }

        public string getBezelFileURL() {
            return BezelFileURL;
        }

        public void setBezelFileURL(string newValue)
        {
            BezelFileURL = newValue;
        }
    }
}