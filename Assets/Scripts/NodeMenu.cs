using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMenu : MonoBehaviour
{
    public string Text { get; set; }
    public string Animation { get; set; }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetText(string text)
    {
        Text = text;
    }
    
    public void SetAnimation(string id)
    {
        Animation = id;
    }
    
    
    public void SetPosition(Vector3 position, GameObject playerCamera)
    {
        gameObject.transform.position = position;
        Vector3 direction = (gameObject.transform.position - playerCamera.transform.position).normalized;
        gameObject.transform.rotation = Quaternion.LookRotation(direction);
    }
}
