using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService
{
    private static readonly string _ApiUrl = "http://157.253.192.192:8080/api";
    
    [Serializable]
    public class Node
    {
        public int index;
        public string text;
        public string animation;
    }

    [Serializable]
    public class Routine
    {
        public List<Node> nodes;
        public List<Vector3> line;
    }

    [Serializable]
    public class Wrapper
    {
        public List<Routine> routines;
    }
    
    public static IEnumerator PostRoutine(Routine routine)
    {
        string jsonData = JsonUtility.ToJson(routine);
    
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(_ApiUrl + "/walk", ""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
        
            yield return www.SendWebRequest();
        
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Post Error: " + www.error + " - Response: " + www.downloadHandler.text);
                yield break;
            }
        
            Debug.Log("Routine posted successfully: " + www.downloadHandler.text);
        }
    }

}