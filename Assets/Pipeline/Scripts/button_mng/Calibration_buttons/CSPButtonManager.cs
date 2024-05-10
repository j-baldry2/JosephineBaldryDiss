using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSPButtonManager : MonoBehaviour
{

    [SerializeField]
    private GameObject canv;
    [SerializeField]
    private Button yourButton;
    [SerializeField]
    private GameObject display;
    [SerializeField]
    private GameObject b1;



    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        display.GetComponent<SaveCalibration>().enabled = false;
        display.SetActive(false);
        b1.SetActive(false);
        canv.GetComponent<Calibration>().enabled = true;
    }

}

