using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameObject playerCamera;
    public PressableButton button;
    public Material planeMaterial;
    
    
    public float lastStepDistance = 0.01f;
    public float planeSize = 0.05f;

    private bool _activated;
    private Vector3 _lastPos;
    
    // Start is called before the first frame update
    void Start()
    {
        _activated = false;
        button.OnClicked.AddListener(OnTriggerFollow);
    }

    void OnTriggerFollow()
    {
        _activated = !_activated;
        _lastPos = GetPathRelativePos();
        AddPoint(_lastPos);
    }

    Vector3 GetPathRelativePos()
    {
        return playerCamera.transform.position + Vector3.down * 1.7f;
    }

    // Update is called once per frame

    void AddPoint(Vector3 newPoint)
    {
        GameObject plane =  GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = newPoint;
        plane.transform.localScale = new Vector3(planeSize, planeSize, planeSize);
        Vector3 euler = playerCamera.transform.rotation.eulerAngles;
        plane.transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        plane.GetComponent<Renderer>().sharedMaterial = planeMaterial;
    }
    
    void Update()
    {
        if (_activated)
        {
            if (Vector3.Distance(GetPathRelativePos(), _lastPos) > lastStepDistance)
            {
                _lastPos = GetPathRelativePos();
                AddPoint(_lastPos);
            }
        }
        button.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        button.transform.rotation = playerCamera.transform.rotation;
    }
}
