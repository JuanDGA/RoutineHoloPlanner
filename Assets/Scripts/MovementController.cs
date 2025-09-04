using System;
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
    
    private static (Vector3 min, Vector3 max) GetBounds(List<Vector3> pts)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var p in pts)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        return (min, max);
    }

    private static float SafeDiv(float a, float b)
    {
        return (Math.Abs(b) < 1e-6f) ? 1f : a / b;
    }
    
    public static List<Vector3> SmoothLine(List<Vector3> points, int windowSize)
    {
        if (points == null || points.Count == 0 || windowSize < 1)
            throw new ArgumentException("Invalid input");

        // Step 1: smooth
        var smoothed = new List<Vector3>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            int start = Math.Max(0, i - windowSize);
            int end = Math.Min(points.Count - 1, i + windowSize);

            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int j = start; j <= end; j++)
            {
                sum += points[j];
                count++;
            }

            smoothed.Add(sum / count);
        }

        // Step 2: compute bounding boxes
        (Vector3 minOrig, Vector3 maxOrig) = GetBounds(points);
        (Vector3 minSmooth, Vector3 maxSmooth) = GetBounds(smoothed);

        Vector3 scale = new Vector3(
            SafeDiv(maxOrig.x - minOrig.x, maxSmooth.x - minSmooth.x),
            SafeDiv(maxOrig.y - minOrig.y, maxSmooth.y - minSmooth.y),
            SafeDiv(maxOrig.z - minOrig.z, maxSmooth.z - minSmooth.z)
        );

        // Step 3: rescale + translate smoothed points
        var adjusted = new List<Vector3>(points.Count);
        for (int i = 0; i < smoothed.Count; i++)
        {
            Vector3 relative = smoothed[i] - minSmooth;
            Vector3 scaled = new Vector3(
                relative.x * scale.x,
                relative.y * scale.y,
                relative.z * scale.z
            );
            Vector3 translated = scaled + minOrig;
            adjusted.Add(translated);
        }

        return adjusted;
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
            List<Vector3> smooth = SmoothLine(_linePositions, _linePositions.Count / 20);
            _linePositions = smooth;
            _currentLine.positionCount = _linePositions.Count;
            _currentLine.SetPositions(_linePositions.ToArray());
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
