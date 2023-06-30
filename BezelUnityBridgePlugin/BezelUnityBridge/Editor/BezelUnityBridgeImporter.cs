using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Bezel.Bridge.Editor.Settings;
using Bezel.Bridge.Editor.Utils;

namespace Bezel.Bridge.Editor.Settings
{
    public static class BezelUnityBridgeImporter
    {
        /// <summary>
        /// The settings asset, containing preferences for importing
        /// </summary>
        private static BezelUnityBridgeSettings s_BezelUnityBridgeSettings;

        private static string bezelAPIUrl = "https://api.bezel.it/v1/objects/?id=public/";

        //// We'll cache the access token in editor Player prefs
        //private const string BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY = "BEZEL_PERSONAL_ACCESS_TOKEN";

        //// Cached personal access token, retrieved from PlayerPrefs
        //private static string s_PersonalAccessToken;

        [MenuItem("Bezel Bridge/Open Settings File")]
        static void SelectSettings()
        {
            var requirementsMet = CheckRequirements();
            var bridgeSettings = BezelUnityBridgeSettingsProvider.FindUnityBridgeSettingsAsset();
            Selection.activeObject = bridgeSettings;
            Debug.Log("Bezel Editor: Open SelectSettings.");
        }

        [MenuItem("Bezel Bridge/Import from Sync Key")]
        static void ImportFromSyncKeyl()
        {
            Debug.Log("Bezel Editor: Download and import from sync key.");
            var requirementsMet = CheckRequirements();
            if (requirementsMet)
            {
                ImportBezelFile(s_BezelUnityBridgeSettings.SyncKey);
            }
        }

        [MenuItem("Bezel Bridge/Load into Unity Hierachy")]
        static void LoadIntoHierachy()
        {
            //Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
            Debug.Log("Bezel Editor: Load into hierachy.");

        }

        [MenuItem("Bezel Bridge/Load into Unity Hierachy", true)]
        static bool ValidateLoadIntoHierachy()
        {
            // Return true if the menu item should be enabled, false if it should be disabled
            return false;
        }

        [MenuItem("Bezel Bridge/Set Personal Access Token")]
        static void SetPersonalAccessToken()
        {
            // Todo: Implement RequestPersonalAccessToken
            Debug.Log("Bezel Editor: Open window for entering token.");
        }

        [MenuItem("Bezel Bridge/Set Personal Access Token", true)]
        static bool ValidateSetPersonalAccessToken()
        {
            // Return true if the menu item should be enabled, false if it should be disabled
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
                if (EditorUtility.DisplayDialog("No Bezel Unity Bridge Settings File",
                        "Create a new Bezel Unity bridge settings file? ", "Create", "Cancel"))
                {
                    s_BezelUnityBridgeSettings =
                        BezelUnityBridgeSettingsProvider.GenerateBezelUnityBridgeSettingsAsset();
                }
                else
                {
                    return false;
                }
            }

            if (s_BezelUnityBridgeSettings.SyncKey.Length == 0)
            {
                EditorUtility.DisplayDialog("Missing Bezel Information", "Bezel file sync key is not valid, please enter valid key", "OK");
                return false;
            }

            if (!Directory.Exists(s_BezelUnityBridgeSettings.FileDirectory)) {
                Directory.CreateDirectory(s_BezelUnityBridgeSettings.FileDirectory);
            }

            //// Todo: Get stored personal access key
            //s_PersonalAccessToken = PlayerPrefs.GetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY);

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Bezel Unity Bridge Importer", "Please exit play mode before importing", "OK");
                return false;
            }

            return true;

        }

        private static async void ImportBezelFile(string _syncKey)
        {
            
            //StartCoroutine(DownloadFileCoroutine(path));

            EditorUtility.DisplayCancelableProgressBar("Importing Bezel File", $"Downloading file", 0);

            try
            {
                await GetBezelFile(_syncKey);
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezel file:" + e.ToString());
            }

            EditorUtility.ClearProgressBar();

        }

        public static async Task<String> GetBezelFile(string syncKey)
        {
            string apiPath = bezelAPIUrl + syncKey;
            string downloadPath = "";
            Debug.Log("API Path: " + apiPath);

            UnityWebRequest webRequest = UnityWebRequest.Get(apiPath);
            ////Todo: Provide access token
            //webRequest.SetRequestHeader("Bezel-Token", s_PersonalAccessToken);

            await webRequest.SendWebRequest();
            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = webRequest.downloadHandler.data;

                    downloadPath = Encoding.UTF8.GetString(data);

                    Debug.Log("Large File Path is Ready: " + downloadPath);

                    await DownloadLargeFileCoroutine(downloadPath);
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError("Path Missing. Error: " + webRequest.error);
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Invalid Sync Key:" + e.ToString());
            }

            return downloadPath;
        }

        public static async Task<String> DownloadLargeFileCoroutine(string s3Path)
        {
            string savePath = s_BezelUnityBridgeSettings.FileDirectory + fileNameFromSyncKey(s_BezelUnityBridgeSettings.SyncKey);

            UnityWebRequest webRequest = UnityWebRequest.Get(s3Path);
             
            // This request is async by default, so won't need await, or the progress won't increase in editor. 
            webRequest.SendWebRequest();

            await WaitForDownloadComplete(webRequest);
            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = webRequest.downloadHandler.data;
                    System.IO.File.WriteAllBytes(savePath, data);
                    Debug.Log("File downloaded successfully.");
                }
                else
                {
                    Debug.LogError("File download failed. Error: " + webRequest.error);
                    EditorUtility.ClearProgressBar();
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Error downloading Bezel file:" + e.ToString());
            }

            return savePath;
        }

        private static async Task WaitForDownloadComplete(UnityWebRequest webRequest)
        {
            while(!webRequest.isDone)
            {
                EditorUtility.DisplayCancelableProgressBar("Importing Bezel File", $"Downloading file", webRequest.downloadProgress);

                await Task.Yield();
            }
        }

        private static string fileNameFromSyncKey(string key)
        {
            string file_name = "bezel";
            string extension = ".gltf";

            file_name = key.Substring(0, key.IndexOf("-")) + extension;

            return file_name;
        }
    }
}