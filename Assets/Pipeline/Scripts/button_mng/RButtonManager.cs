using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RButtonManager : MonoBehaviour
{

    [SerializeField]
    private GameObject canv;
    [SerializeField]
    private Button yourButton;
    [SerializeField]
    private RawImage display;
    

    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        display.GetComponent<OpenCVSaveFrames>().StartRecord();
        Debug.Log("here");
    }

}
