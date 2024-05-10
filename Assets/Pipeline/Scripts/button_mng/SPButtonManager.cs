using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPButtonManager : MonoBehaviour
{

    [SerializeField]
    private GameObject canv;
    [SerializeField]
    private Button yourButton;
    [SerializeField]
    private GameObject display;
    [SerializeField]
    private GameObject b1, b2;
    [SerializeField]
    private GameObject movey;



    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        display.GetComponent<OpenCVSaveFrames>().enabled = false;
        movey.GetComponent<activate>().activateit = true;
        display.SetActive(false);
        b1.SetActive(false);
        b2.SetActive(false);
        canv.GetComponent<SFM>().enabled = true;
    }

}

