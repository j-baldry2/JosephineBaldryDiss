using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Transform camera;
    public Transform moveDude;

    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;

    private Quaternion cameraCentre;
    // Start is called before the first frame update
    void Start()
    {
        cameraCentre = camera.localRotation; //Set rotation origin for camera to cam centre
    }

    // Update is called once per frame
    void Update()
    {
        // Lock cursor - might need to put this somewhere else later
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetY();
        SetX();
    }

    void SetY()
    {
        float p_input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(p_input, -Vector3.right);
        Quaternion delta = camera.localRotation * adj;

        if (Quaternion.Angle(cameraCentre, delta) < maxAngle)
        {
            camera.localRotation = delta;
        }
    }

    void SetX()
    {
        float p_input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(p_input, Vector3.up);
        Quaternion delta = moveDude.localRotation * adj;
        moveDude.localRotation = delta;
    }
}
