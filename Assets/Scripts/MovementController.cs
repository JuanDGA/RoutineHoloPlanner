using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameObject playerCamera;
    public PressableButton button;
    public GameObject cube;

    private bool _activated;

    private Vector3 _lastPos;
    
    private readonly Vector3 _sphereScale = new Vector3(0.1f, 0.1f, 0.1f);
    private readonly Material _sphereMaterial = new Material();
    
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
        GameObject sphere =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = newPoint;
        sphere.transform.localScale = _sphereScale;

    }
    
    void Update()
    {
        if (_activated)
        {
            cube.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2;
            if (Vector3.Distance(GetPathRelativePos(), _lastPos) > 0.1f)
            {
                _lastPos = GetPathRelativePos();
                AddPoint(_lastPos);
            }
        }
        button.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        button.transform.rotation = playerCamera.transform.rotation;
    }
}
