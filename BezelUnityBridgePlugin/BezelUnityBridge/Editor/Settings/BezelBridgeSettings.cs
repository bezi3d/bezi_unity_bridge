using UnityEngine;

namespace Bezel.Bridge.Editor.Settings
{
	public class BezelBridgeSettings : ScriptableObject
	{
        [Tooltip("The Bezel glTF file to import")]
        public string BezelGLTFFilePath;

        [Tooltip("Generate logic and linking of Bezel scene based on glTF's 'extras' information")]
        public bool BuildPrototypeFlow = true;
    }

}