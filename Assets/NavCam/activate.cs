using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activate : MonoBehaviour
{
    public bool activateit;
    public GameObject thingy;
    // Start is called before the first frame update
    void Start()
    {
        activateit = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (activateit)
        {
            thingy.GetComponent<Movement>().enabled = true;
            thingy.GetComponent<NavCam>().enabled = true;
        }
    }
}
