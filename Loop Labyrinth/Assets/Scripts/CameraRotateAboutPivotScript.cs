using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CameraRotateAboutPivotScript makes the main camera orbit around a target with a mouse drag. You can also zoom in and out with the scroll wheel. I modified this from code I copied from the internet.
public class CameraRotateAboutPivotScript : MonoBehaviour
{

    public float xRot;
    public float yRot;

    int xRotDirection = 1;
    int yRotDirection = 1;

    public float distance = 21f;
    public float mouse_sensitivity = 1000f;
    public Transform target;

    void Start()
    {
        //This ensures that camera rotation remains constant after mouse click/drag, but the position of the camera may change after mouse click/drag.
        xRot = -this.transform.eulerAngles.x;
        yRot = this.transform.eulerAngles.y - 180;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            //gets x and y rotation according to mouse position, taking into account sensitivity
            xRot += Input.GetAxis("Mouse Y") * mouse_sensitivity * Time.deltaTime * xRotDirection;
            yRot += Input.GetAxis("Mouse X") * mouse_sensitivity * Time.deltaTime * yRotDirection;

            //to account for the camera flipping around when xRot = +/-90f
            if (xRot > 89f)
            {
                xRot = 89f;
            }
            else if (xRot < -89f)
            {
                xRot = -89f;
            }

            //rotates camera towards pivot
            transform.position = target.position + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
            transform.LookAt(target.position, Vector3.up);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            if(Input.mouseScrollDelta.y > 0 || (Input.mouseScrollDelta.y < 0 && distance > 0)){
                distance += Input.mouseScrollDelta.y;
            }
            if(distance < 0){
                distance = 0;
            }
            //rotates camera towards pivot
            transform.position = target.position + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
            transform.LookAt(target.position, Vector3.up);
        }
    }
}
