using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PlayerHeadUi : MonoBehaviour
{
    private Camera mCamera;

    private void Start()
    {
        this.mCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (this.mCamera)
        {
            this.transform.forward = this.mCamera.transform.forward;
            this.transform.rotation = this.mCamera.transform.rotation;
        }
    }
}



