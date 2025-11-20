using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathBuilder
{
    private static Vector3 _transformPosition;
    private static Quaternion _transformRotation;
    
    private static bool _activated;
    private static Vector3 _lastPos;
    private static float _currentY;
    private static LineRenderer _currentLine;
    private static List<Vector3> _linePositions = new();
    private static List<int> _nodes = new(); // Indices of nodes in _linePositions
    private static List<GameObject> _nodesMenu = new();
    private static List<GameObject> _nodeObjects = new();
    
    private static Vector3 _smoothedPos;

    private static GameObject _playerCamera;
    private static Material _pathsMaterial;
    private static float _planeSize;
    private static GameObject _linesParent;
    private static float _lastStepDistance;
    private static GameObject _nodeMenuPrefab;

    public static void SetUp(GameObject playerCamera, Material pathsMaterial, float planeSize, GameObject linesParent, float lastStepDistance, GameObject nodeMenuPrefab)
    {
        _playerCamera = playerCamera;
        _pathsMaterial = pathsMaterial;
        _planeSize = planeSize;
        _linesParent = linesParent;
        _lastStepDistance = lastStepDistance;
        _nodeMenuPrefab = nodeMenuPrefab;
    }

    public static void Restore()
    {
        // Properly clean up the current line
        if (_currentLine != null)
        {
            GameObject.Destroy(_currentLine.gameObject);
            _currentLine = null;
        }
        
        // Clean up all node menus and objects
        for (int i = 0; i < _nodesMenu.Count; i++)
        {
            GameObject nodeMenu = _nodesMenu[i];
            GameObject nodeObject = _nodeObjects[i];
            if (nodeMenu != null) GameObject.Destroy(nodeMenu);
            if (nodeObject != null) GameObject.Destroy(nodeObject);
        }

        // Reset all lists and state
        _nodesMenu = new List<GameObject>();
        _nodeObjects = new List<GameObject>();
        _nodes = new List<int>();
        _linePositions = new List<Vector3>();
        _activated = false;
        _lastPos = Vector3.zero;
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

    public static (List<Vector3>, List<ApiService.Node>) CurrentPath()
    {
        Debug.Log("Nodes: " + _nodes.Count + "|" + _nodesMenu.Count + "|" + _nodesMenu.Count);
        List<ApiService.Node> nodes = new List<ApiService.Node>();
        List<Vector3> convertedPositions = new();
        
        for (int i = 0; i < _nodes.Count; i++)
        {
            var n =  _nodes[i];
            var node = new ApiService.Node
            {
                index = n,
                text = _nodesMenu[i].GetComponent<NodeMenu>().Text,
                animation = _nodesMenu[i].GetComponent<NodeMenu>().Animation,
            };
            nodes.Add(node);
        }

        for (int i = 0; i < _linePositions.Count; i++)
        {
            // Transform the world position to the local coordinate system
            Vector3 worldPosition = _linePositions[i];
        
            // First translate to origin (subtract reference position)
            Vector3 translatedPosition = worldPosition - _transformPosition;
        
            // Then apply inverse rotation to align with reference rotation
            Vector3 localPosition = Quaternion.Inverse(_transformRotation) * translatedPosition;
        
            convertedPositions.Add(localPosition);
        }
        
        return (convertedPositions, nodes);
    }

    public static void Activate()
    {
        if (_activated) return;
        if (_linePositions == null) _linePositions = new List<Vector3>();
        if (_nodes == null) _nodes = new List<int>();
        _currentY = GetPathRelativePos().y;
        CreateLine(); // TODO: Refactor line generation logic
        _activated = true;
    }

    public static void AddNode(float sizeMultiplier = 1.0f)
    {
        if (!_activated) return;
        var (nodeIdx, position) = Update(true);
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        node.transform.localScale = new Vector3(_planeSize * 3 * sizeMultiplier, _planeSize * 0.1f * sizeMultiplier, _planeSize * 3 * sizeMultiplier);
        node.transform.position = position;
        node.GetComponent<Renderer>().sharedMaterial = _pathsMaterial;
        node.transform.SetParent(_linesParent.transform);
        _nodesMenu.Add(MainStateHandler.Instantiate(_nodeMenuPrefab, position + Vector3.up * 1.5f, Quaternion.identity));
        _nodeObjects.Add(node);
        _nodes.Add(nodeIdx);
    }

    public static void Deactivate()
    {
        if (!_activated) return;
        _activated = false;
    }

    private static void CreateLine()
    {
        _linePositions.Clear();
        _nodes.Clear();
        if (_currentLine != null) return; 
        _currentLine = new GameObject().AddComponent<LineRenderer>();
        _currentLine.material = _pathsMaterial;
        _currentLine.startWidth = _planeSize;
        _currentLine.endWidth = _planeSize;
        _currentLine.transform.SetParent(_linesParent.transform);
    }

    public static void SetReference(Vector3 transformPosition, Quaternion transformRotation)
    {
        _transformPosition = transformPosition;
        _transformRotation = transformRotation;
    }

    private static Vector3 GetPathRelativePos()
    {
        return _playerCamera.transform.position + Vector3.down * 1.7f;
    }

    private static (int, Vector3) AddPoint(Vector3 newPoint)
    {
        newPoint.y = _currentY;
        _linePositions.Add(newPoint);
        _currentLine.positionCount = _linePositions.Count;
        _currentLine.SetPositions(_linePositions.ToArray());
        return (_linePositions.Count - 1, newPoint);
    }
    
    public static (int, Vector3) Update(bool addingNode = false)
    {
        if (!_activated)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                GameObject nodeMenu = _nodesMenu[i];
                nodeMenu.SetActive(true);
                nodeMenu.GetComponent<NodeMenu>().SetPosition(_linePositions[_nodes[i]] + Vector3.up * 1.5f, _playerCamera);
            }
            return (-1, Vector3.zero);
        }
        
        for (int i = 0; i < _nodes.Count; i++)
        {
            GameObject nodeMenu = _nodesMenu[i];
            nodeMenu.SetActive(false);
        }
        
        Vector3 rawPos = GetPathRelativePos();
        rawPos.y = _currentY;

        if (addingNode || Vector3.Distance(rawPos, _lastPos) > _lastStepDistance)
        {
            _lastPos = rawPos;
            return AddPoint(_lastPos);
        }

        return (-1, Vector3.zero);
    }
}
