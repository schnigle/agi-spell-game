using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREmulator : MonoBehaviour
{
    [SerializeField]
    GameObject controller;
    [SerializeField]
    GameObject spellbook;
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
            cameraRotationAccel *= cameraLookSpeed;
            cameraRotationSpeed += cameraRotationAccel * Time.deltaTime * Time.deltaTime * 60;
            cameraRotationSpeed = Vector3.Lerp(cameraRotationSpeed, Vector3.zero, Time.deltaTime * 10);
            VRCamera.transform.rotation = Quaternion.Euler(VRCamera.transform.rotation.eulerAngles + cameraRotationSpeed);
            if (controller)
            {
                controller.transform.position = VRCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, armLength));
                controller.transform.LookAt(VRCamera.transform.position);
                controller.transform.rotation =  Quaternion.Euler(controller.transform.rotation.eulerAngles + new Vector3(180 + scepterRotationOffset, -scepterRotationOffset, 0));
                
            }
            if (spellbook)
            {
                spellbook.transform.eulerAngles = new Vector3
                (
                    -20,
                    VRCamera.transform.eulerAngles.y,
                    spellbook.transform.eulerAngles.z
                );
                // spellbook.transform.rotation = VRCamera.transform.rotation;
                var camDir = VRCamera.transform.forward;
                camDir.y = 0;
                spellbook.transform.position = transform.position + camDir * 0.4f + transform.up * 0.2f;
            }
        }
    }
}
