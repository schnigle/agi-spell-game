using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.IO;

public class PlayerScript : MonoBehaviour
{
    public GameObject rightStick;
    [SerializeField]
    Camera VRcamera;


    Vector3 moveVector;
    CharacterController controller;

    public SteamVR_Input_Sources rightInput = SteamVR_Input_Sources.RightHand;

    StreamWriter fileStream;

    PlayerData playerData;

    public PlayerData GetPlayerData()
    {
        if (playerData == null)
            playerData = new PlayerData();
        return playerData;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        // camera = (GameObject)GameObject.Instantiate(CameraPrefab);
        // camera.transform.SetParent(transform);
        // camera.transform.localPosition = Vector3.zero;
        // camera.transform.localRotation = Quaternion.identity;

        hand = rightStick.GetComponent<Valve.VR.InteractionSystem.Hand>();
        if(playerData == null)
            playerData = new PlayerData();


    }

    public SteamVR_Action_Boolean grabPinch; //Grab Pinch is the trigger, select from inspecter
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;//which controller

    private Valve.VR.InteractionSystem.Hand hand;

    // Update is called once per frame
    bool trigger_down_last = false;

    void Update()
    {
        playerData.customUpdater();
        bool trigger_down = SteamVR_Actions._default.Squeeze.GetAxis(rightInput) == 1;
        if (trigger_down)
        {
            if (!trigger_down_last)
            {
                String timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                print(timeStamp);
                fileStream = new StreamWriter(@"d:\AGI19Projects\spellwave\output\" + timeStamp + "_testspell.txt", true);
            }
            //print("Left trigger value: " + SteamVR_Actions._default.Squeeze.GetAxis(leftInput));
            /*print(Time.time + " "
                + rightStick.transform.position.x + " "
                + rightStick.transform.position.y + " "
                + rightStick.transform.position.z
            );*/
            var pixelPos = VRcamera.WorldToScreenPoint(rightStick.transform.position);
            var height = 1600;
            var width = 2880 / 2;
            //print("cam space: " + pixelPos.x / Screen.width + ", " + pixelPos.y / Screen.height);
            string line = Time.time + " " + pixelPos.x / width + " " + pixelPos.y / height + " " + pixelPos.z;
            print(line);
            // fileStream
            fileStream.WriteLine(line);
            fileStream.Flush();
        }
        else if (trigger_down_last)
        {
            fileStream.Flush();
            fileStream.Close();
        }
        

        //REeset the MoveVector
        moveVector = Vector3.zero;

        //Check if cjharacter is grounded
        if (controller.isGrounded == false)
        {
            //Add our gravity Vecotr
            moveVector += Physics.gravity;
        }

        //Apply our move Vector , remeber to multiply by Time.delta
        controller.Move(moveVector * Time.deltaTime);

        trigger_down_last = trigger_down;
    }
}
