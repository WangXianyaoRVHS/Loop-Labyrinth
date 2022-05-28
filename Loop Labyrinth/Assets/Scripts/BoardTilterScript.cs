using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BoardTilterScript tilts the labyrinth board in response to player input (WASD). I modified it from code I copied from the internet.
public class BoardTilterScript : MonoBehaviour {

	public float smooth = 20.0f;    // How smoothly the board rotates from one rotation to the next
    public float tiltAngle = 20.0f; // How much the board rotates
    public GameObject MainCamera;   // The camera that provides the view in the scene. We need this so that the board tilts in the right direction relative to the direction of the camera(which has an orbiting script).
    float tiltAroundZ;              // How much the board is tilted along the Z axis.
    float tiltAroundX;              // How much the board is tilted along the X axis.

    float yRot;                     // How much the camera is rotated along the Y axis.
    
    void start() {
        yRot = (Mathf.Abs(MainCamera.GetComponent<CameraRotateAboutPivotScript>().yRot) + 45) % 360; // Retrieve the yRot variable from the orbiting script from the MainCamera. Process it by taking the modulus and moduloing it by 360. 45 degrees is added to it
    }

    void Update()
    {
        if (!Input.anyKey) { yRot = (Mathf.Abs(MainCamera.GetComponent<CameraRotateAboutPivotScript>().yRot) + 45) % 360; } // Only update the yRot variable when there is no keyboard or mouse button input.  This is so that you can hold down a WASD key while rotating a cemra while the board still rotates in the same direction.
        //Debug.Log("BoardTilterScript yROT: " + yRot.ToString());

        // Smoothly tilts a transform towards a target rotation.
        // This depends on what quadrant the camera is facing.
        if (90 <= yRot && yRot < 180) {
            tiltAroundZ = Input.GetAxis("Vertical") * -tiltAngle;       // Get the horizontal and vertical axis. Modified by tiltAngle, which is negative to control the direction of rotation.
            tiltAroundX = Input.GetAxis("Horizontal") * -tiltAngle;
        }

        if (180 <= yRot && yRot < 270) {
            tiltAroundZ = Input.GetAxis("Horizontal") * -tiltAngle;
            tiltAroundX = Input.GetAxis("Vertical") * tiltAngle;
        }

        if (270 <= yRot && yRot < 360) {
            tiltAroundZ = Input.GetAxis("Vertical") * tiltAngle;
            tiltAroundX = Input.GetAxis("Horizontal") * tiltAngle;
        }

        if (0 <= yRot && yRot < 90) {
            tiltAroundZ = Input.GetAxis("Horizontal") * tiltAngle;
            tiltAroundX = Input.GetAxis("Vertical") * -tiltAngle;
        }
        

        Quaternion target = Quaternion.Euler(tiltAroundX, 0, tiltAroundZ); // The new rotation to reotate towards.

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
    }
}
