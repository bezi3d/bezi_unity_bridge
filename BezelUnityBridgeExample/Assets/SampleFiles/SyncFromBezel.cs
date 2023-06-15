using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class SyncFromBezel : MonoBehaviour
{
    private string bezelAPIUrl = "https://api.bezel.it/v1/objects/?id=public/";
    public string syncKey;
    private string fileDirectory = "Assets/SampleFiles/";
    private string fileName = "bezel.gltf";

    public bool download = false;

    [SerializeField]
    private string downloadPercentage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (download) {
            DownloadFile();
            download = false;
        }
    }

    public void DownloadFile()
    {
        string path = bezelAPIUrl + syncKey;

        Debug.Log("API Path: " + path);
        StartCoroutine(DownloadFileCoroutine(path));
    }

    private string fileNameFromSyncKey(string key) {
        string file_name = "bezel";
        string extension = ".gltf";


        file_name = key.Substring(0, key.IndexOf("--"))+extension;
        return file_name;
    }
   
    private IEnumerator DownloadFileCoroutine(string apiPath)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiPath))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] data = webRequest.downloadHandler.data;

                string result = Encoding.UTF8.GetString(data);

                Debug.Log("Large File Path is Ready: " + result);

                StartCoroutine(DownloadLargeFileCoroutine(result));
            }
            else
            {
                Debug.LogError("Path Missing. Error: " + webRequest.error);
            }
        }
    }

    private IEnumerator DownloadLargeFileCoroutine(string s3Path)
    {
        string savePath = fileDirectory + fileNameFromSyncKey(syncKey);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(s3Path))
        {
            //yield return webRequest.SendWebRequest();
            webRequest.SendWebRequest();


            while (!webRequest.isDone)
            {
                // Calculate the download percentage
                float percentage = webRequest.downloadProgress * 100f;

                // Update your UI or display the percentage
                downloadPercentage = percentage.ToString("F2") + "%";

                yield return null;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] data = webRequest.downloadHandler.data;
                System.IO.File.WriteAllBytes(savePath, data);

                Debug.Log("File downloaded successfully.");
                downloadPercentage = "100%";
            }
            else
            {
                Debug.LogError("File download failed. Error: " + webRequest.error);
            }
        }
    }

}
