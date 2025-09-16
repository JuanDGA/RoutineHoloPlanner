using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;

public class RoutineController : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject linesParent;
    public Material nodesMaterial;
    public AudioSource audioSource;

    private TextToSpeechSubsystem _tts;
    
    public float planeSize = 0.05f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _tts = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();
    }

    void OnEnable()
    {
        PathsController.onTrackStarts += HandleStartTracking;
        PathsController.onTrackEnds += HandleEndTracking;
    }


    void OnDisable()
    {
        PathsController.onTrackStarts -= HandleStartTracking;
        PathsController.onTrackEnds -= HandleEndTracking;
    }


    void Speak(string text)
    {
        _tts.TrySpeak(text, audioSource);
    } 


    void HandleStartTracking(Vector3 position)
    {
        if (!MainStateHandler.DoingTutorial)
            Speak("Recording movement. Walk across the path that you want the robot to follow");
    }


    void HandleEndTracking(List<Vector3> positions)
    {
        if (!MainStateHandler.DoingTutorial)
            Speak("Recording stopped");
    }

    void CreateNode(Vector3 pos)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        node.transform.localScale = new Vector3(planeSize * 3, planeSize * 0.3f, planeSize * 3);
        node.transform.position = pos;
        node.GetComponent<Renderer>().sharedMaterial = nodesMaterial;
        node.transform.SetParent(linesParent.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
