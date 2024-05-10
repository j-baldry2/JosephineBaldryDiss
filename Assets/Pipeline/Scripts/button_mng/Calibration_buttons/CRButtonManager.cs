using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CRButtonManager : MonoBehaviour
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
        display.GetComponent<SaveCalibration>().Capture();
        Debug.Log("here");

    }

}
