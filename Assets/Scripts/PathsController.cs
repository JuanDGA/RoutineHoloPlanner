using System;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using Unity.VisualScripting;
using UnityEngine;

public class PathsController
{
    private static bool _activated;
    private static Vector3 _lastPos;
    private static float _currentY;
    private static LineRenderer _currentLine;
    private static List<Vector3> _linePositions;
    
    private static Vector3 _smoothedPos;

    public static bool IsActivated()
    {
        return _activated;
    }

    public static float CurrentPathDistance(float limit = Single.PositiveInfinity)
    {
        if (_linePositions.Count <= 1) return 0.0f;
        if (_linePositions.Count == 2) return (_linePositions[0] - _linePositions[1]).magnitude;
        float totalDistance = 0;
        for (int i = 1; i < _linePositions.Count; i++)
        {
            totalDistance += (_linePositions[i] - _linePositions[i - 1]).magnitude;
            if (totalDistance >= limit) return totalDistance;
        }

        return totalDistance;
    }

    public static List<Vector3> CurrentPath()
    {
        return _linePositions;
    }

    public static void Activate(GameObject playerCamera, Material pathsMaterial, float planeSize, GameObject linesParent)
    {
        if (_activated) return;
        if (_linePositions == null) _linePositions = new List<Vector3>();
        _lastPos = GetPathRelativePos(playerCamera);
        _linePositions.Add(_lastPos);
        CreateLine(pathsMaterial, planeSize, linesParent);
        AddPoint(_lastPos);
        _activated = true;
    }

    public static void Deactivate(GameObject playerCamera, bool reset = true)
    {
        if (!_activated) return;
        _activated = false;
        _lastPos = GetPathRelativePos(playerCamera);
        _linePositions.Add(_lastPos);
        if (reset) _linePositions =  new List<Vector3>();
    }

    private static void CreateLine(Material pathsMaterial, float planeSize, GameObject linesParent)
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
    
    private static List<Vector3> SmoothLine(List<Vector3> points, int windowSize)
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

    private static Vector3 GetPathRelativePos(GameObject playerCamera)
    {
        return playerCamera.transform.position + Vector3.down * 1.7f;
    }

    private static void AddPoint(Vector3 newPoint)
    {
        newPoint.y = _currentY;
        _linePositions.Add(newPoint);
        _currentLine.positionCount = _linePositions.Count;
        _currentLine.SetPositions(_linePositions.ToArray());
    }
    
    public static void Compute(GameObject playerCamera, float smoothFactor, float lastStepDistance)
    {
        if (!_activated) return;
        
        Vector3 rawPos = GetPathRelativePos(playerCamera);
        _smoothedPos = Vector3.Lerp(_smoothedPos, rawPos, smoothFactor);

        if (Vector3.Distance(_smoothedPos, _lastPos) > lastStepDistance)
        {
            _lastPos = _smoothedPos;
            AddPoint(_lastPos);
        }
    }
}
