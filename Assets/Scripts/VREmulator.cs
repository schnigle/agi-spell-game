using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREmulator : MonoBehaviour
{
    [SerializeField]
    GameObject controller;
    [SerializeField]
    Camera VRCamera;
    [SerializeField]
    float cameraLookSpeed = 15;
    [SerializeField]
    float armLength = 0.5f;
    [SerializeField]
    float scepterRotationOffset = 5;

    Vector3 cameraRotationSpeed = new Vector3();

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Valve.VR.SteamVR.active)
        {
            enabled = false;
        }
        else
        {
            VRCamera.transform.position += Vector3.up * 1.8f;
            VRCamera.nearClipPlane = 0.03f;
        }
    }

    bool left, right;

    // Update is called once per frame
    void Update()
    {
        if (VRCamera)
        {
            Vector3 cameraRotationAccel = new Vector3();
            if (Input.GetKey("w"))
            {
                cameraRotationAccel.x -= 1;
            }
            if (Input.GetKey("s"))
            {
                cameraRotationAccel.x += 1;
            }
            if (Input.GetKeyDown("a"))
            {
                left = true;
                //cameraRotationAccel.y -= 1;
            }
            if (Input.GetKeyDown("d"))
            {
                right = true;
                //cameraRotationAccel.y += 1;
            }
            if (Input.GetKeyUp("a"))
            {
                left = false;
            }
            if (Input.GetKeyUp("d"))
            {
                right = false;
            }
            if (right)
            {
                cameraRotationAccel.y += 10;
            }
            if (left)
            {
                cameraRotationAccel.y -= 10;
            }

            cameraRotationAccel *= cameraLookSpeed * Time.deltaTime;
            cameraRotationSpeed += cameraRotationAccel;
            cameraRotationSpeed = Vector3.Lerp(cameraRotationSpeed, Vector3.zero, Time.deltaTime * 10);
            VRCamera.transform.rotation = Quaternion.Euler(VRCamera.transform.rotation.eulerAngles + cameraRotationSpeed);
            if (controller)
            {
                controller.transform.position = VRCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, armLength));
                controller.transform.LookAt(VRCamera.transform.position);
                controller.transform.rotation =  Quaternion.Euler(controller.transform.rotation.eulerAngles + new Vector3(180 + scepterRotationOffset, -scepterRotationOffset, 0));
                
            }
        }
    }
}
