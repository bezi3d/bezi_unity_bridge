using UnityEngine;

namespace Bezi.Bridge.Editor
{
	public class BeziUnityBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezi File Link")]
        public string FileLink = "";

        private string SyncKey = "";

        [Tooltip("The Unity Project Folder")]
        private string FileDirectory = "Assets/BeziFiles/";

        public string FontsFolderName = "Fonts";

        public string FontMaterialPresetsFolderName = "FontMaterialPresets";

        public string BeziFileURL = "";

        private bool devMode = false;

        private bool showInHierarchy = false;

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

        public string getBeziFileURL() {
            return BeziFileURL;
        }

        public void setBeziFileURL(string newValue)
        {
            BeziFileURL = newValue;
        }

        public bool getDevMode() {
            return devMode;
        }

        public void setDevMode(bool newValue)
        {
            devMode = newValue;
        }

        public bool getShowInHierarchy()
        {
            return showInHierarchy;
        }

        public void setShowInHierarchy(bool newValue)
        {
            showInHierarchy = newValue;
        }
    }
}