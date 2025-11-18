using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using UnityEngine;

public class PepperVisualization
{
    private bool _visualizing = false;
    private int _step = 0;
    private Vector3 _position;
    private List<Vector3> _path = new();
    private Vector3 _transformPosition;
    private Quaternion _transformRotation;
    
    public void SetReference(Vector3 transformPosition, Quaternion transformRotation)
    {
        _transformPosition = transformPosition;
        _transformRotation = transformRotation;
    }
    
    private Vector3 GetWorldPosition(Vector3 localPosition)
    {
        Vector3 rotatedOffset = _transformRotation * localPosition;
        return _transformPosition + rotatedOffset;
    }
    
    public void StartVisualization(List<Vector3> path, GameObject pepperRobot)
    {
        _path = path;
        _step = 0;
        _position = path[0];
        _visualizing = true;
        pepperRobot.transform.position = GetWorldPosition(_position);
    }

    public void StopVisualization()
    {
        _visualizing = false;
        _step = 0;
        _path = new();
    }
    
    public void UpdateVisualization(float deltaTime, GameObject pepperRobot)
    {
        if (!_visualizing) return;
        if (_step >= _path.Count - 1) return;
        Vector3 target = GetWorldPosition(_path[_step]);
        
        target.y = pepperRobot.transform.position.y;
        if (pepperRobot.transform.position == target)
        {
            _step += 1;
            return;
        }
        target.y = pepperRobot.transform.position.y;

        var direction = target - pepperRobot.transform.position;
        
        pepperRobot.transform.rotation = Quaternion.LookRotation(direction);
        pepperRobot.transform.Rotate(Vector3.up * 90);
        pepperRobot.transform.position = Vector3.MoveTowards(pepperRobot.transform.position, target, deltaTime * 0.5f);
    }
}