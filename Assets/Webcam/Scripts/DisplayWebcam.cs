using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;

public class DisplayWebcam : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.RawImage _rawImage;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        // for debugging purposes, prints available devices to the console
        for (int i = 0; i < devices.Length; i++)
        {
            print("Webcam available: " + devices[i].name);
        }

        Renderer rend = this.GetComponentInChildren<Renderer>();

        // assuming the first available WebCam is desired
        WebCamTexture tex = new WebCamTexture(devices[1].name);
        rend.material.mainTexture = tex;
        tex.Play();
    }
}

/*
 * https://stackoverflow.com/questions/19482481/display-live-camera-feed-in-unity
 * https://docs.unity3d.com/ScriptReference/WebCamTexture.html
 * 
 */