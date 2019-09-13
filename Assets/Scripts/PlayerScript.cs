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
    [SerializeField]
    GameObject trajectory;
    [SerializeField]
    GameObject trail;


    Vector3 moveVector;
    CharacterController controller;

    public SteamVR_Input_Sources rightInput = SteamVR_Input_Sources.RightHand;

    PlayerData playerData;

    bool spellReady = false;
    GestureRecognition.Gesture identifiedGesture;

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

        trajectory.SetActive(false);
        trail.SetActive(false);
    }

    public SteamVR_Action_Boolean grabPinch; //Grab Pinch is the trigger, select from inspecter
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;//which controller

    private Valve.VR.InteractionSystem.Hand hand;
    GestureRecognition gestureRecognition = new GestureRecognition();
    // Update is called once per frame
    bool trigger_down_last = false;
    List<GestureRecognition.Point_2D> gesture = new List<GestureRecognition.Point_2D>();

    void Update()
    {
        

        playerData.customUpdater();
        bool trigger_down = SteamVR_Actions._default.Squeeze.GetAxis(rightInput) == 1;
        
        if (trigger_down)
        {
            if (!trigger_down_last)
            {
                if(spellReady)
                {
                    GetComponent<Spell>().UnleashSpell();
                    spellReady = false;
                    trajectory.SetActive(false);
                }
                trail.SetActive(true);
                trail.GetComponent<TrailRenderer>().Clear();
            }
            //print("Left trigger value: " + SteamVR_Actions._default.Squeeze.GetAxis(leftInput));
            /*print(Time.time + " "
                + rightStick.transform.position.x + " "
                + rightStick.transform.position.y + " "
                + rightStick.transform.position.z
            );*/
            var pixelPos = VRcamera.WorldToScreenPoint(rightStick.transform.position);
            //var height = 1600; // TODO D:
            //var width = 2880 / 2;
            var height = 1200;
            var width = 2160 / 2;
            
            //print("cam space: " + pixelPos.x / Screen.width + ", " + pixelPos.y / Screen.height);
            string line = Time.time + " " + pixelPos.x / width + " " + pixelPos.y / height + " " + pixelPos.z;
            print(line);
            // fileStream
            var point = new GestureRecognition.Point_2D();
            point.time = Time.time;
            point.x = pixelPos.x / width;
            point.y = pixelPos.y / height;
            point.z = pixelPos.z;
            gesture.Add(point);
        }
        else if (trigger_down_last)
        {
            trail.SetActive(false);
            GestureRecognition.Gesture result = gestureRecognition.recognize_gesture(gesture);
            print(result);
            identifiedGesture = result;
            if (identifiedGesture != GestureRecognition.Gesture.unknown)
            {
                spellReady = true;
                trajectory.SetActive(true);
            }
            gesture.Clear();
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
