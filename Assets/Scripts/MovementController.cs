using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameObject playerCamera;
    public PressableButton button;
    public GameObject cube;

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
        lastPos = GetPathRelativePos();
        AddPoint(lastPos);
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
        sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
    
    void Update()
    {
        if (activated)
        {
            cube.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2;
            if (Vector3.Distance(GetPathRelativePos(), lastPos) > 0.1f)
            {
                lastPos = GetPathRelativePos();
                AddPoint(lastPos);
            }
        }
        button.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        button.transform.rotation = playerCamera.transform.rotation;
    }
}
