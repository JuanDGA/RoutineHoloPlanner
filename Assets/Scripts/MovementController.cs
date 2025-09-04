using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.Serialization;

public class MovementController : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject linesParent;
    public Material pathsMaterial;
    
    public PressableButton triggerButton;
    
    public float lastStepDistance = 0.01f;
    public float planeSize = 0.05f;

    private bool _activated;
    private Vector3 _lastPos;
    private List<LineRenderer> _lines;
    private LineRenderer _currentLine;
    private float _currentY;
    private List<Vector3> _linePositions;
    
    // Start is called before the first frame update
    void Start()
    {
        _activated = false;
        triggerButton.OnClicked.AddListener(OnTriggerFollow);
        
        _lines = new List<LineRenderer>();
        _linePositions = new List<Vector3>();
    }

    void CreateLine()
    {
        _linePositions.Clear();
        _currentLine = new GameObject().AddComponent<LineRenderer>();
        _currentLine.material = pathsMaterial;
        _currentLine.startWidth = planeSize;
        _currentLine.endWidth = planeSize;
        _currentLine.transform.SetParent(linesParent.transform);
    }

    void CreateNode(Vector3 pos)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        node.transform.localScale = new Vector3(planeSize * 3, planeSize * 0.1f, planeSize * 3);
        node.transform.position = pos;
        node.GetComponent<Renderer>().sharedMaterial = pathsMaterial;
        node.transform.SetParent(linesParent.transform);
    }

    void OnTriggerFollow()
    {
        _lastPos = GetPathRelativePos();
        _activated = !_activated;
        if (!_activated) // Save and create new one
        {
            _lastPos.y = _currentY;
            CreateNode(_lastPos);
            _lines.Add(_currentLine);
        }
        else
        {
            _currentY = _lastPos.y;
            CreateNode(_lastPos);
            CreateLine();
            AddPoint(_lastPos);
        }
    }

    Vector3 GetPathRelativePos()
    {
        return playerCamera.transform.position + Vector3.down * 1.7f;
    }

    // Update is called once per frame

    void AddPoint(Vector3 newPoint)
    {
        newPoint.y = _currentY;
        _linePositions.Add(newPoint);
        _currentLine.positionCount = _linePositions.Count;
        _currentLine.SetPositions(_linePositions.ToArray());
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
    }
}
