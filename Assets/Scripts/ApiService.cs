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
        public string routine_name;
        public List<Node> nodes;
        public List<Vector3> line;
    }

    [Serializable]
    public class Wrapper
    {
        public List<Routine> routines;
    }
    
    // Internal classes for API deserialization
    [Serializable]
    private class PointData
    {
        public float x;
        public float y;
        public float z;
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
    
    [Serializable]
    private class RoutineData
    {
        public int id;
        public string routine_name;
        public List<Node> nodes;
        public List<PointData> points;
        public long timestamp;
        
        public Routine ToRoutine()
        {
            var routine = new Routine
            {
                routine_name = routine_name,
                nodes = nodes ?? new List<Node>(),
                line = new List<Vector3>()
            };
            
            if (points != null)
            {
                foreach (var point in points)
                {
                    routine.line.Add(point.ToVector3());
                }
            }
            
            return routine;
        }
    }
    
    [Serializable]
    private class WrapperData
    {
        public List<RoutineData> routines;
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
                String message = "API Get Error: " + www.error + " - Response: " + www.downloadHandler.text;
                OnRequestError?.Invoke(message);
                Debug.LogError(message);
                yield break;
            }

            try
            {
                Debug.Log("Raw API Response: " + www.downloadHandler.text);
                
                // Deserialize using the internal WrapperData structure
                WrapperData wrapperData = JsonUtility.FromJson<WrapperData>(www.downloadHandler.text);
                
                // Convert to the public Routine format
                List<Routine> routines = new List<Routine>();
                if (wrapperData.routines != null)
                {
                    foreach (var routineData in wrapperData.routines)
                    {
                        routines.Add(routineData.ToRoutine());
                    }
                    
                    Debug.Log($"Successfully parsed {routines.Count} routines");
                    foreach (var routine in routines)
                    {
                        Debug.Log($"Routine: {routine.routine_name}, Points: {routine.line?.Count ?? 0}, Nodes: {routine.nodes?.Count ?? 0}");
                    }
                }
                else
                {
                    Debug.LogWarning("No routines found in API response");
                }
                
                OnRoutinesReceived?.Invoke(routines);
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

