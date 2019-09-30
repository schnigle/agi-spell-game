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
        }
    }

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
            if (Input.GetKey("a"))
            {
                cameraRotationAccel.y -= 1;
            }
            if (Input.GetKey("d"))
            {
                cameraRotationAccel.y += 1;
            }
            cameraRotationAccel *= cameraLookSpeed * Time.deltaTime;
            cameraRotationSpeed += cameraRotationAccel;
            cameraRotationSpeed = Vector3.Lerp(cameraRotationSpeed, Vector3.zero, Time.deltaTime * 10);
            VRCamera.transform.rotation = Quaternion.Euler(VRCamera.transform.rotation.eulerAngles + cameraRotationSpeed);
            if (controller)
            {
                controller.transform.position = VRCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, armLength));
                controller.transform.LookAt(VRCamera.transform.position);
                controller.transform.rotation =  Quaternion.Euler(controller.transform.rotation.eulerAngles + new Vector3(180 + 5, -5, 0));
                
            }
        }
    }
}
