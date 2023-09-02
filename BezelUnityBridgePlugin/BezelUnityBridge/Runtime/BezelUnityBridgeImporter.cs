using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Bezel.Bridge.Editor.Utils;
using static UnityEditor.Progress;

#pragma warning disable CS4014 // webRequest.SendWebRequest() is not awaited intentionally

namespace Bezel.Bridge.Editor.Settings
{
    public static class BezelUnityBridgeImporter
    {
        /// <summary>
        /// The settings asset, containing preferences for importing
        /// </summary>
        private static BezelUnityBridgeSettings s_BezelUnityBridgeSettings;

        private static string bezelAPIUrl = "https://api.bezel.it/v1/objects/?";

        // We'll cache the access token in editor Player prefs
        public const string BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY = "BEZEL_PERSONAL_ACCESS_TOKEN";
        private const string BEZEL_SYNC_KEY_NAME = "SyncKey";
        private const string BEZEL_ACCESS_TOKEN_NAME = "BezelToken";

        public static string s_PersonalAccessToken;
        private static string downloadFilePath;
        private static string bezelExtras;

        public static GameObject importedGameObject;


        [MenuItem("Bezel Bridge/Open Bezel Settings Menu")]
        static void SelectSettings()
        {
            var requirementsMet = CheckRequirements();
            var bridgeSettings = BezelUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            Selection.activeObject = bridgeSettings;
        }

        public static async void ImportFromSyncKey()
        {
            var requirementsMet = CheckRequirements();
            if (!requirementsMet) return;

            bool result = await ImportBezelFile(s_BezelUnityBridgeSettings.FileLink);

            //Trigger importing glTF after downloading 
            if (!result)
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
            }

            // Import assets to show under Assets folder
            AssetDatabase.ImportAsset(downloadFilePath);

            importedGameObject = (GameObject)AssetDatabase.LoadAssetAtPath(downloadFilePath, typeof(GameObject));

            EditorUtility.DisplayDialog("Import Ready", "Click next button to visual " + fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), false) + ".prefab from " + s_BezelUnityBridgeSettings.getFileDirectory() + " into Hierarchy", "Okay");
        }

        public static void ConstructBezelObject()
        {
            if (importedGameObject != null)
            {
                GameObject bezelPrefabTemp = PrefabUtility.InstantiatePrefab(importedGameObject) as GameObject;

                // Open up prefab to add componenet
                PrefabUtility.UnpackPrefabInstance(bezelPrefabTemp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                BezelGLTFConstructor.ObjectsContructor(bezelPrefabTemp, bezelExtras);

                GameObject bezelPrefabModified = PrefabUtility.SaveAsPrefabAsset(bezelPrefabTemp, s_BezelUnityBridgeSettings.getFileDirectory() + fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), false) + ".prefab");

                GameObject bezelPrefabInHierarchy = PrefabUtility.InstantiatePrefab(bezelPrefabModified) as GameObject;

                bezelPrefabInHierarchy.transform.position = new Vector3(0, 0, 0);

                // Remove temporary prefab
                GameObject.DestroyImmediate(bezelPrefabTemp);

                EditorUtility.DisplayDialog("Import Success", fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), false) + ".prefab is in Hierarchy", "Okay");
            }
            else
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
            }
        }

        public static void ConstructText()
        {
            if (importedGameObject != null)
            {
                //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                //AssetDatabase.ImportAsset(downloadFilePath);

                EditorUtility.DisplayDialog("Import Success", "Go to folder "+ s_BezelUnityBridgeSettings.getFileDirectory() + ", and drag the file " + fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), true) + " into Hierarchy", "Okay");
            }
            else
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
            }
        }

        public static bool RequestPersonalAccessToken()
        {
            s_PersonalAccessToken = EditorPrefs.GetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY);

            if (!string.IsNullOrEmpty(s_PersonalAccessToken))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Check to make sure all requirements are met before syncing
        /// </summary>
        /// <returns></returns>
        private static bool CheckRequirements()
        {
            // Find the settings asset if it exists
            if (s_BezelUnityBridgeSettings == null)
            {
                s_BezelUnityBridgeSettings = BezelUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            if (s_BezelUnityBridgeSettings == null)
            {
                if (EditorUtility.DisplayDialog("Welcome to Bezel Bridge!",
                        "Let's create a new Bezel Bridge settings file? ", "Create", "Cancel"))
                {
                    s_BezelUnityBridgeSettings =
                        BezelUnityBridgeSettingsProvider.GenerateBezelUnityBridgeSettingsAsset();
                }
                else
                {
                    return false;
                }
            }

            s_PersonalAccessToken = PlayerPrefs.GetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY);

            if (string.IsNullOrEmpty(s_PersonalAccessToken))
            {
                var setToken = RequestPersonalAccessToken();
                if (!setToken) {
                    EditorUtility.DisplayDialog("Step 1: Enter Bezel Access Token", "Access token can be created under Bezel's account setting or Share panel. It ensures file security. ", "Enter Access Token");
                    return false;
                }
            }

            if (s_BezelUnityBridgeSettings.getFileLink().Length == 0)
            {
                EditorUtility.DisplayDialog("Step 2: Enter Bezel File Link", "After Step 1 (access token), file link can be copied for each Bezel file under Share panel.", "Enter File Link");
                return false;
            }

            if (!Directory.Exists(s_BezelUnityBridgeSettings.getFileDirectory()))
            {
                EditorUtility.DisplayDialog("Step 3: Setup Unity File Path", "After Step 2 (file link), the imported file will be saved at: " +
                                            s_BezelUnityBridgeSettings.getFileDirectory() + ". You can change the path as well.", "Create Folder");
                Directory.CreateDirectory(s_BezelUnityBridgeSettings.getFileDirectory());

                Directory.CreateDirectory(GetFontsFolder());

                Directory.CreateDirectory(GetFontMaterialPresetsFolder());
            }

            if (Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
            {
                EditorUtility.DisplayDialog("Text Mesh Pro", "You need to install TestMeshPro Essentials. Use Window->Text Mesh Pro->Import TMP Essential Resources", "STOP");
                
                return false;
            }


            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Bezel Unity Bridge Importer", "Please exit play mode before importing", "OK");
                return false;
            }

            return true;

        }

        private static async Task<bool> ImportBezelFile(string _fileLink)
        {
            EditorUtility.DisplayCancelableProgressBar("Importing Bezel File", "Downloading file to " + s_BezelUnityBridgeSettings.getFileDirectory(), 0);

            s_BezelUnityBridgeSettings.setSyncKey(convertFileLinkToSyncKey(_fileLink));

            try
            {
                await GetBezelFile(s_BezelUnityBridgeSettings.getSyncKey());
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezel file:" + e.ToString());
                return false;
            }

            EditorUtility.ClearProgressBar();

            return true;
        }

        
        public static async Task<String> GetBezelFile(string syncKey)
        {
            string apiPath = bezelAPIUrl + BEZEL_SYNC_KEY_NAME + "=" + syncKey + "&" +
                                            BEZEL_ACCESS_TOKEN_NAME + "=" + s_PersonalAccessToken +
                                            (s_BezelUnityBridgeSettings.getDevMode() ? "&DevMode=1" : "");
            string api_response = "";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiPath);

            await webRequest.SendWebRequest();
            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = webRequest.downloadHandler.data;

                    api_response = Encoding.UTF8.GetString(data);

                    var result = JsonUtility.FromJson<API_Response>(api_response);

                    s_BezelUnityBridgeSettings.setBezelFileURL(result.bezelUrl);

                    await DownloadLargeFileCoroutine(result.download);
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Import Error", "Access token or file link may be invalid. Please try copy both strings again. ", "STOP");
                }
            }
            catch
            {
                EditorUtility.ClearProgressBar();
            }

            return api_response;
        }

        public static async Task<bool> DownloadLargeFileCoroutine(string s3Path)
        {
            string savePath = s_BezelUnityBridgeSettings.getFileDirectory() + fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), true);

            UnityWebRequest webRequest = UnityWebRequest.Get(s3Path);

            // This request is async by default, so won't need await, or the progress won't increase in editor. 
            webRequest.SendWebRequest();

            await WaitForDownloadComplete(webRequest);

            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = webRequest.downloadHandler.data;

                    // Read json to get bezel_objects
                    // Convert to string 
                    string jsonString = System.Text.Encoding.UTF8.GetString(data);

                    GLTFSchema json = JsonConvert.DeserializeObject<GLTFSchema>(jsonString);

                    if (json.extras == null)
                    {
                        Debug.Log("No valid bezel extras");
                        return false;
                    }

                    bezelExtras = json.extras.ToString();

                    byte[] dataBezelObjects = System.Text.Encoding.UTF8.GetBytes(bezelExtras);

                    System.IO.File.WriteAllBytes(s_BezelUnityBridgeSettings.getFileDirectory() + fileNameFromSyncKey(s_BezelUnityBridgeSettings.getSyncKey(), false) + ".json", dataBezelObjects);
                    
                    System.IO.File.WriteAllBytes(savePath, data);

                    downloadFilePath = savePath;
                }
                else
                {
                    Debug.LogError("File download failed. Error: " + webRequest.error);
                    EditorUtility.ClearProgressBar();
                    return false;
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezel file:" + e.ToString());
            }

            return true;
        }

        public static string getBezelFolder()
        {
            return s_BezelUnityBridgeSettings.getFileDirectory();
        }

        public static string GetFontsFolder()
        {
            if (s_BezelUnityBridgeSettings == null)
            {
                s_BezelUnityBridgeSettings = BezelUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            return s_BezelUnityBridgeSettings.getFileDirectory() + "/"+s_BezelUnityBridgeSettings.FontsFolderName;
        }

        public static string GetFontMaterialPresetsFolder()
        {
            if (s_BezelUnityBridgeSettings == null)
            {
                s_BezelUnityBridgeSettings = BezelUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            return s_BezelUnityBridgeSettings.getFileDirectory() + "/" + s_BezelUnityBridgeSettings.FontMaterialPresetsFolderName;
        }

        private static async Task WaitForDownloadComplete(UnityWebRequest webRequest)
        {
            while(!webRequest.isDone)
            {
                EditorUtility.DisplayCancelableProgressBar("Importing Bezel File", "Downloading file to "+ s_BezelUnityBridgeSettings.getFileDirectory(), webRequest.downloadProgress);

                await Task.Yield();
            }
        }

        private static string convertFileLinkToSyncKey(string fileLink)
        {
            string _syncKey;

            var parts = fileLink.Split('/');
            _syncKey = parts[parts.Length - 1];

            return _syncKey;
        }

        private static string fileNameFromSyncKey(string key, bool withExtension)
        {
            string file_name = "bezel";
            string extension = ".gltf";

            file_name = key.Substring(0, key.IndexOf("-")) + ((withExtension)?extension:"");

            return file_name;
        }
    }

    [System.Serializable]
    internal class API_Response
    { 
        public string download;
        public string bezelUrl;
    }
}