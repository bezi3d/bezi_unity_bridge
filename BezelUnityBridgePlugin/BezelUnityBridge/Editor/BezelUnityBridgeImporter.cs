using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Bezel.Bridge;
using Bezel.Bridge.Editor.Settings;
using Bezel.Bridge.Editor.Utils;
using System.Net;
using System.Security.Policy;

namespace Bezel.Bridge.Editor.Settings
{
    public static class BezelUnityBridgeImporter
    {
        /// <summary>
        /// The settings asset, containing preferences for importing
        /// </summary>
        private static BezelUnityBridgeSettings s_BezelUnityBridgeSettings;

        private static string bezelAPIUrl = "https://api.bezel.it/v1/objects/?id=public/";
        private static string bezelAPIUrl_ = "https://api.bezel.it/v1/objects/?";

        // We'll cache the access token in editor Player prefs
        private const string BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY = "BEZEL_PERSONAL_ACCESS_TOKEN";
        private const string BEZEL_SYNC_KEY_NAME = "SyncKey";
        private const string BEZEL_ACCESS_TOKEN_NAME = "BezelToken";

        // Cached personal access token, retrieved from PlayerPrefs
        private static string s_PersonalAccessToken;

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
            Debug.Log("Bezel Editor: Open window for entering token.");
            RequestPersonalAccessToken();
        }

        [MenuItem("Bezel Bridge/Set Personal Access Token", true)]
        static bool ValidateSetPersonalAccessToken()
        {
            // Return true if the menu item should be enabled, false if it should be disabled
            return true;
        }

        static bool RequestPersonalAccessToken()
        {
            s_PersonalAccessToken = PlayerPrefs.GetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY);
            var newAccessToken = EditorInputDialog.Show("Personal Access Token", "Please enter your Bezel Personal Access Token (you can create in the 'Account Settings' page)", s_PersonalAccessToken);
            if (!string.IsNullOrEmpty(newAccessToken))
            {
                s_PersonalAccessToken = newAccessToken;
                Debug.Log($"New access token set {s_PersonalAccessToken}");
                PlayerPrefs.SetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY, s_PersonalAccessToken);
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

            s_PersonalAccessToken = PlayerPrefs.GetString(BEZEL_PERSONAL_ACCESS_TOKEN_PREF_KEY);

            if (string.IsNullOrEmpty(s_PersonalAccessToken))
            {
                var setToken = RequestPersonalAccessToken();
                if (!setToken) return false;
            }

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Bezel Unity Bridge Importer", "Please exit play mode before importing", "OK");
                return false;
            }

            return true;

        }

        private static async void ImportBezelFile(string _syncKey)
        {
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
            string apiPath = bezelAPIUrl_ +  BEZEL_SYNC_KEY_NAME + "=" + syncKey + "&" +
                                            BEZEL_ACCESS_TOKEN_NAME + "=" + s_PersonalAccessToken;
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
                    Debug.LogError("Request Error: " + webRequest.error);
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Invalid Sync Key:" + e.ToString());
            }

            return api_response;
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

    [System.Serializable]
    internal class API_Response
    { 
        public string download;
        public string bezelUrl;
    }
}