using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCam : MonoBehaviour
{
    [SerializeField]
    private Transform camera;
    [SerializeField]
    private Rigidbody moveDude;

    [SerializeField]
    private float speed = 800f;
    private Quaternion cameraCentre;

    // Start is called before the first frame update
    void Start()
    {
        moveDude = GetComponent<Rigidbody>();
        cameraCentre = camera.localRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Movement
        float x_move = Input.GetAxisRaw("Horizontal");
        float z_move = Input.GetAxisRaw("Vertical");
        Debug.Log(x_move);

        float y_move = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            y_move = 1;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            y_move = -1;
        }
        Vector3 move = new Vector3(x_move, y_move, z_move);
        move.Normalize();

        Vector3 targetVelocity = transform.TransformDirection(move) * speed * Time.deltaTime;
        moveDude.velocity = targetVelocity;


    }
}
