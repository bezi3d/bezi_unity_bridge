using UnityEngine;

namespace Bezel.Bridge.Editor.Settings
{
	public class BezelUnityBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezel File Link")]
        public string FileLink = "";

        private string SyncKey = "";

        [Tooltip("The Unity Project Folder")]
        private string FileDirectory = "Assets/BezelFiles/";

        public string FontsFolderName = "Fonts";

        public string FontMaterialPresetsFolderName = "FontMaterialPresets";

        public string BezelFileURL = "";

        private bool devMode = false;

        public string getFileLink()
        {
            return FileLink;
        }

        public void setFileLink(string newValue)
        {
            FileLink = newValue;
        }

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

        public bool getDevMode() {
            return devMode;
        }

        public void setDevMode(bool newValue)
        {
            devMode = newValue;
        }
    }
}