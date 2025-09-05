using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService
{
    private static readonly string _ApiUrl = "http://localhost:5000/api";
    
    [Serializable]
    public class Node
    {
        public string dialog;
        public string animation;
        public Node nextNode;
        public List<Vector3> pathToNextNode;
    }

    [Serializable]
    public class Routine
    {
        public string name;
        public string description;
        public List<Node> nodes;
        public DateTime CreatedAt;
    }

    [Serializable]
    public class Wrapper
    {
        public List<Routine> routines;
    }
    
    public IEnumerator GetRoutines(Action<List<Routine>> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_ApiUrl + "/routines"))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Error: " + www.error);
                onComplete?.Invoke(null);
                yield break;
            }
            
            string jsonResult = www.downloadHandler.text;
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonResult);
            
            onComplete?.Invoke(wrapper.routines);
        }
    }
}