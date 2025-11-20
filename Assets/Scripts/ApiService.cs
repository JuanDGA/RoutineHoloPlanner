using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService
{
    private static readonly string _ApiUrl = "https://routineholoplannerworker.neoventix.workers.dev/api/data";
    
    public static event Action<List<Routine>> OnRoutinesReceived;
    public static event Action<String> OnRequestError;
    
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
    
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(_ApiUrl, ""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
        
            yield return www.SendWebRequest();
        
            if (www.result != UnityWebRequest.Result.Success)
            {
                String message = "API Post Error: " + www.error + " - Response: " + www.downloadHandler.text;
                OnRequestError?.Invoke(message);
                Debug.LogError(message);
                yield break;
            }
        
            Debug.Log("Routine posted successfully: " + www.downloadHandler.text);
        }
    }

    public static IEnumerator GetRoutines()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_ApiUrl))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
        
            yield return www.SendWebRequest();
        
            if (www.result != UnityWebRequest.Result.Success)
            {
                String message = "API Post Error: " + www.error + " - Response: " + www.downloadHandler.text;
                OnRequestError?.Invoke(message);
                Debug.LogError(message);
                yield break;
            }

            try
            {
                Wrapper wrapper = JsonUtility.FromJson<Wrapper>(www.downloadHandler.text);
                
                OnRoutinesReceived?.Invoke(wrapper.routines);

                Debug.Log("Routine posted successfully: " + www.downloadHandler.text);
            }
            catch (Exception e)
            {
                String message = "API Response Parsing Error: " + e.Message + " - Response: " + www.downloadHandler.text;
                OnRequestError?.Invoke(message);
                Debug.LogError(message);
            }
        }
    }

    public static void Clean()
    {
        OnRequestError = null;
        OnRoutinesReceived = null;
    }
}