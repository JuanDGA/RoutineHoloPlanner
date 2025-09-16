using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;

public class MainStateHandler : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject mainCamera;

    private bool _doingTutorial = false;
    private bool _completed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        mainMenu.transform.position = mainCamera.transform.forward * 2.0f + mainCamera.transform.position;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
