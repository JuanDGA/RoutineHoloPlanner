using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameObject playerCamera;
    public PressableButton button;
    public GameObject cube;
    public LineRenderer line;

    private bool activated;

    private Vector3 lastPos;
    
    // Start is called before the first frame update
    void Start()
    {
        activated = false;
        button.OnClicked.AddListener(OnTriggerFollow);
    }

    void OnTriggerFollow()
    {
        activated = !activated;
        line.positionCount = 0;
        lastPos = playerCamera.transform.position + Vector3.down * 1.2f;
        AddPoint(lastPos);
    }

    // Update is called once per frame

    void AddPoint(Vector3 newPoint)
    {
        int index = line.positionCount;
        line.positionCount = index + 1;
        line.SetPosition(index, newPoint);
    }
    
    void Update()
    {
        if (activated)
        {
            cube.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2;
            if (Vector3.Distance(playerCamera.transform.position, lastPos) > 0.1f)
            {
                lastPos = playerCamera.transform.position + Vector3.down * 1.2f;
                AddPoint(lastPos);
            }
        }
        button.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        button.transform.rotation = playerCamera.transform.rotation;
    }
}
