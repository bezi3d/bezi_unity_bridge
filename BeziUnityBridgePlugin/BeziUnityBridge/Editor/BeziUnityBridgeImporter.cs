using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Bezi.Bridge.Editor.Utils;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

#pragma warning disable CS4014 // webRequest.SendWebRequest() is not awaited intentionally

namespace Bezi.Bridge.Editor
{
    public static class BeziUnityBridgeImporter
    {
        /// <summary>
        /// The settings asset, containing preferences for importing
        /// </summary>
        private static BeziUnityBridgeSettings s_BeziUnityBridgeSettings;

        private static string beziAPIUrl = "https://api.bezel.it/v1/objects/?"; // TODO: rename to bezi.com after backend updates

        // We'll cache the access token in editor Player prefs
        public const string BEZI_PERSONAL_ACCESS_TOKEN_PREF_KEY = "BEZI_PERSONAL_ACCESS_TOKEN";
        private const string BEZI_SYNC_KEY_NAME = "SyncKey";
        private const string BEZI_SYNC_KEY_PREFIX = "bezel_"; // TODO: rename to bezi_ after backend migration.
        private const string BEZI_ACCESS_TOKEN_NAME = "BezelToken"; // TODO: rename to bezi_ after backend migration.

        public static string s_PersonalAccessToken;
        private static string downloadFilePath;
        private static string beziExtras;

        public static GameObject importedGameObject;

        public static GameObject beziPrefabNew;

        private static bool glTFastReady = true;
        private static ListRequest requestPackages;


        [MenuItem("Bezi Bridge/Open Bezi Settings Menu")]
        static void SelectSettings()
        {
            var requirementsMet = CheckRequirements();
            var bridgeSettings = BeziUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            Selection.activeObject = bridgeSettings;
        }

        public static async void ImportFromSyncKey()
        {
            var requirementsMet = CheckRequirements();
            if (!requirementsMet) return;

            bool result = await ImportBeziFile(s_BeziUnityBridgeSettings.FileLink);

            if (!glTFastReady) return;

            //Trigger importing glTF after downloading 
            if (!result)
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
                return;
            }
            
            // Download text resources
            await BeziGLTFConstructor.PreparBeziTextResources(beziExtras);

            // Import assets to show under Assets folder
            AssetDatabase.ImportAsset(downloadFilePath + ".json");
            AssetDatabase.ImportAsset(downloadFilePath + ".gltf");

            importedGameObject = (GameObject)AssetDatabase.LoadAssetAtPath(downloadFilePath + ".gltf", typeof(GameObject));
            if (!importedGameObject)
            {
                EditorUtility.DisplayDialog("Import Error", "Make sure glTFast is installed.", "STOP");
                return;
            }

            CreatePrefabInFolder();

            EditorUtility.DisplayDialog("Import Success", fileNameFromSyncKey(s_BeziUnityBridgeSettings.getSyncKey(), false) + ".prefab is in the folder: " + s_BeziUnityBridgeSettings.getFileDirectory(), "Okay");

            if (s_BeziUnityBridgeSettings.getShowInHierarchy())
            {
                BringPrefabIntoHierarchy();
            }
        }

        public static void CreatePrefabInFolder()
        {
            if (importedGameObject != null)
            {
                GameObject beziPrefabTemp = PrefabUtility.InstantiatePrefab(importedGameObject) as GameObject;

                // Open up prefab to add componenet
                PrefabUtility.UnpackPrefabInstance(beziPrefabTemp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                BeziGLTFConstructor.ObjectsContructor(beziPrefabTemp, beziExtras);

                beziPrefabNew = PrefabUtility.SaveAsPrefabAsset(beziPrefabTemp, downloadFilePath + ".prefab");

                // Remove temporary prefab
                GameObject.DestroyImmediate(beziPrefabTemp);
            }
            else
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
            }
        }

        public static void BringPrefabIntoHierarchy()
        {
            if (importedGameObject != null)
            {
                EditorUtility.DisplayDialog("Prefab in Hierarchy", "Click OKAY to bring " + fileNameFromSyncKey(s_BeziUnityBridgeSettings.getSyncKey(), false) + ".prefab from " + s_BeziUnityBridgeSettings.getFileDirectory() + " into Hierarchy", "OKAY");

                GameObject beziPrefabInHierarchy = PrefabUtility.InstantiatePrefab(beziPrefabNew) as GameObject;

                beziPrefabInHierarchy.transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                EditorUtility.DisplayDialog("Import Error", "Encounter import issue.", "STOP");
            }
        }

        public static bool RequestPersonalAccessToken()
        {
            s_PersonalAccessToken = EditorPrefs.GetString(BEZI_PERSONAL_ACCESS_TOKEN_PREF_KEY);

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
            if (s_BeziUnityBridgeSettings == null)
            {
                s_BeziUnityBridgeSettings = BeziUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            if (s_BeziUnityBridgeSettings == null)
            {
                if (EditorUtility.DisplayDialog("Welcome to Bezi Bridge!",
                        "Let's create a new Bezi Bridge settings file? ", "Create", "Cancel"))
                {
                    s_BeziUnityBridgeSettings =
                        BeziUnityBridgeSettingsProvider.GenerateBeziUnityBridgeSettingsAsset();
                }
                else
                {
                    return false;
                }
            }

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Bezi Unity Bridge Importer", "Please exit play mode before importing", "OK");
                return false;
            }

            // Check if TMP is installed
            if (Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
            {
                bool tmpPopup = EditorUtility.DisplayDialog("Text Mesh Pro", "You need to install TestMeshPro Essentials. Use Window->Text Mesh Pro->Import TMP Essential Resources", "Install", "Cancel");

                if (tmpPopup)
                {
                    EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
                }

                return false;
            }

            // Check if glTFast is installed
            requestPackages = UnityEditor.PackageManager.Client.List(true); // Set 'true' to show all versions
            EditorApplication.update += CheckglTFastPackageStatus;

            s_PersonalAccessToken = PlayerPrefs.GetString(BEZI_PERSONAL_ACCESS_TOKEN_PREF_KEY);

            if (string.IsNullOrEmpty(s_PersonalAccessToken))
            {
                var setToken = RequestPersonalAccessToken();
                if (!setToken) {
                    EditorUtility.DisplayDialog("Step 1: Enter Bezi Access Token", "Access token can be created under Bezi's account setting or Share panel. It ensures file security. ", "Enter Access Token");
                    return false;
                }
            }

            if (s_BeziUnityBridgeSettings.getFileLink().Length == 0)
            {
                EditorUtility.DisplayDialog("Step 2: Enter Bezi File Link", "After Step 1 (access token), file link can be copied for each Bezi file under Share panel.", "Enter File Link");
                return false;
            }

            if (!Directory.Exists(s_BeziUnityBridgeSettings.getFileDirectory()))
            {
                EditorUtility.DisplayDialog("Step 3: Setup Unity File Path", "After Step 2 (file link), the imported file will be saved at: " +
                                            s_BeziUnityBridgeSettings.getFileDirectory() + ". You can change the path as well.", "Create Folder");
                Directory.CreateDirectory(s_BeziUnityBridgeSettings.getFileDirectory());

                Directory.CreateDirectory(GetFontsFolder());

                Directory.CreateDirectory(GetFontMaterialPresetsFolder());
            }

            return true;
        }

        private static async Task<bool> ImportBeziFile(string _fileLink)
        {
            bool result = false;

            EditorUtility.DisplayCancelableProgressBar("Importing Bezi File", "Downloading file to " + s_BeziUnityBridgeSettings.getFileDirectory(), 0);

            s_BeziUnityBridgeSettings.setSyncKey(convertFileLinkToSyncKey(_fileLink));

            try
            {
                result = await GetBeziFile(s_BeziUnityBridgeSettings.getSyncKey());
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezi file:" + e.ToString());
                return result;
            }

            EditorUtility.ClearProgressBar();

            return result;
        }

        
        public static async Task<bool> GetBeziFile(string syncKey)
        {
            string apiPath = beziAPIUrl + BEZI_SYNC_KEY_NAME + "=" + syncKey + "&" +
                                            BEZI_ACCESS_TOKEN_NAME + "=" + s_PersonalAccessToken +
                                            (s_BeziUnityBridgeSettings.getDevMode() ? "&DevMode=1" : "");
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

                    s_BeziUnityBridgeSettings.setBeziFileURL(result.beziUrl);

                    await DownloadLargeFileCoroutine(result.download);
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Import Error", "Access token or file link may be invalid. Please try copy both strings again. ", "STOP");
                    return false;
                }
            }
            catch
            {
                EditorUtility.ClearProgressBar();
            }

            return true;
        }

        public static async Task<bool> DownloadLargeFileCoroutine(string s3Path)
        {
            string savePath = s_BeziUnityBridgeSettings.getFileDirectory() + fileNameFromSyncKey(s_BeziUnityBridgeSettings.getSyncKey(), false);

            UnityWebRequest webRequest = UnityWebRequest.Get(s3Path);

            // This request is async by default, so won't need await, or the progress won't increase in editor. 
            webRequest.SendWebRequest();

            await WaitForDownloadComplete(webRequest);

            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = webRequest.downloadHandler.data;

                    // Read json to get bezel_objects // TODO: rename to bezi_objects after backend migration.
                    // Convert to string 
                    string jsonString = System.Text.Encoding.UTF8.GetString(data);

                    GLTFSchema json = JsonConvert.DeserializeObject<GLTFSchema>(jsonString);

                    if (json.extras == null)
                    {
                        Debug.Log("No valid bezi extras");
                        return false;
                    }

                    beziExtras = json.extras.ToString();

                    byte[] dataBeziObjects = System.Text.Encoding.UTF8.GetBytes(beziExtras);

                    System.IO.File.WriteAllBytes(savePath + ".json", dataBeziObjects);
                    
                    System.IO.File.WriteAllBytes(savePath + ".gltf", data);

                    downloadFilePath = savePath;
                }
                else
                {
                    Debug.LogError("File download aborted. Reason: " + webRequest.error);
                    EditorUtility.ClearProgressBar();
                    return false;
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezi file:" + e.ToString());
            }

            return true;
        }

        public static string getBeziFolder()
        {
            return s_BeziUnityBridgeSettings.getFileDirectory();
        }

        public static string GetFontsFolder()
        {
            if (s_BeziUnityBridgeSettings == null)
            {
                s_BeziUnityBridgeSettings = BeziUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            return s_BeziUnityBridgeSettings.getFileDirectory() + "/"+s_BeziUnityBridgeSettings.FontsFolderName;
        }

        public static string GetFontMaterialPresetsFolder()
        {
            if (s_BeziUnityBridgeSettings == null)
            {
                s_BeziUnityBridgeSettings = BeziUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            }

            return s_BeziUnityBridgeSettings.getFileDirectory() + "/" + s_BeziUnityBridgeSettings.FontMaterialPresetsFolderName;
        }

        private static async Task WaitForDownloadComplete(UnityWebRequest webRequest)
        {
            while(!webRequest.isDone)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Importing Bezi File", "Downloading file to " + s_BeziUnityBridgeSettings.getFileDirectory(), webRequest.downloadProgress)) {
                    webRequest.Abort();
                }

                if (!glTFastReady) {
                    webRequest.Abort();
                }

                await Task.Yield();
            }
        }

        private static string convertFileLinkToSyncKey(string fileLink)
        {
            string _syncKey;

            var parts = fileLink.Split('/');
            _syncKey = BEZI_SYNC_KEY_PREFIX + parts[parts.Length - 1];

            return _syncKey;
        }

        private static string fileNameFromSyncKey(string key, bool withExtension)
        {
            string file_name = "bezi";
            string extension = ".gltf";

            file_name = key.Substring(0, key.IndexOf("-")) + ((withExtension)?extension:"");

            return file_name;
        }

        private static void CheckglTFastPackageStatus()
        {
            if (requestPackages == null)
            {
                // Request was disposed; stop checking
                EditorApplication.update -= CheckglTFastPackageStatus;
                return;
            }

            if (requestPackages.IsCompleted)
            {
                if (requestPackages.Status == StatusCode.Success)
                {
                    bool isReady = false;
                    foreach (var package in requestPackages.Result)
                    {
                        if (package.name == "com.atteneder.gltfast") // Replace with the package name you're checking
                        {
                            isReady = true;
                            break;
                        }
                    }

                    if (isReady) {
                        glTFastReady = true;
                    }
                    else
                    {
                        glTFastReady = false; // Set glTFastReady globally to notify/abort the download process.

                        EditorUtility.ClearProgressBar();

                        bool openLink = EditorUtility.DisplayDialog("Have glTFast?", "Please install glTFast in. ", "Get glTFast", "Cancel");

                        if (openLink) {
                            string urlToOpen = "https://github.com/atteneder/glTFast#installing";
                            Application.OpenURL(urlToOpen);
                        }

                        //Debug.Log("glTFast package is not installed.");
                    }
                }
                else
                {
                    Debug.LogError("Error checking bezi dependency: " + requestPackages.Error.message);
                }

                // Dispose of the request and remove the event handler
                requestPackages = null;
                EditorApplication.update -= CheckglTFastPackageStatus;
            }
        }

        private static void OnDestroy()
        {
            // Make sure to remove the event handler when the window is destroyed
            EditorApplication.update -= CheckglTFastPackageStatus;
        }
    }

    [System.Serializable]
    internal class API_Response
    { 
        public string download;
        public string beziUrl;
    }
}