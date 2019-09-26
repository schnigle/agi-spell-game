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

    // audio
    public AudioClip ambient_sounds;
    public AudioSource self_audio_source;

    Vector3 moveVector;
    CharacterController controller;

    public SteamVR_Input_Sources rightInput = SteamVR_Input_Sources.RightHand;

    PlayerData playerData;

    bool spellReady = false;
    GestureRecognition.Gesture identifiedGesture;
    ISpell selectedSpell = null;

    public PlayerData GetPlayerData()
    {
        if (playerData == null)
            playerData = new PlayerData();
        return playerData;
    }

    // Start is called before the first frame update
    void Start()
    {
        //audio
        self_audio_source.clip = ambient_sounds;
        self_audio_source.Play();

        controller = GetComponent<CharacterController>();

        hand = rightStick.GetComponent<Valve.VR.InteractionSystem.Hand>();
        if(playerData == null)
            playerData = new PlayerData();

        trajectory.SetActive(false);
        trail.SetActive(false);

        try
        {
            if(SteamVR.instance.hmd_ModelNumber == "Vive MV")
                resolution = new Tuple<int, int>(2160 / 2, 1200);
            else
                resolution = new Tuple<int, int>(2880 / 2, 1600);
        }
        catch (System.Exception)
        {
            Debug.Log("No VR headset has been identified. Defaulting to Vive MV configuration.");
            resolution = new Tuple<int, int>(2160 / 2, 1200);
        }
    }

    private Tuple<int, int> resolution;

    public SteamVR_Action_Boolean grabPinch; //Grab Pinch is the trigger, select from inspecter
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;//which controller

    private Valve.VR.InteractionSystem.Hand hand;
    GestureRecognition gestureRecognition = new GestureRecognition();
    // Update is called once per frame
    bool trigger_down_last = false;
    List<GestureRecognition.Point_2D> gesture = new List<GestureRecognition.Point_2D>();
    List<GestureRecognition.Point_3D> gesture3D = new List<GestureRecognition.Point_3D>();

    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
        playerData.customUpdater();
        bool trigger_down = SteamVR_Actions._default.Squeeze.GetAxis(rightInput) == 1;

        if (trigger_down)
        {
            // Trigger press start
            if (!trigger_down_last)
            {
                if(spellReady && selectedSpell != null)
                {
                    selectedSpell.OnAimEnd();
                    selectedSpell.UnleashSpell();
                    selectedSpell = null;
                    spellReady = false;
                }
                trail.SetActive(true);
                trail.GetComponent<TrailRenderer>().Clear();
            }

            // Record gesture
            var pos = rightStick.transform.position;
            var pixelPos = VRcamera.WorldToScreenPoint(pos);

            var point = new GestureRecognition.Point_2D();
            var point_3D = new GestureRecognition.Point_3D();
            point.time = Time.time;
            point_3D.time = point.time;

            point.x = pixelPos.x / resolution.Item1;
            point.y = pixelPos.y / resolution.Item2;
            point.z = pixelPos.z;

            point_3D.x = pos.x;
            point_3D.y = pos.y;
            point_3D.z = pos.z;

            gesture.Add(point);
            gesture3D.Add(point_3D);
        }
        // Trigger press end
        else if (trigger_down_last)
        {
            trail.SetActive(false);
            GestureRecognition.Gesture_Meta result = gestureRecognition.recognize_gesture(gesture, gesture3D);
            identifiedGesture = result.type;
            foreach (var spell in GetComponents<ISpell>())
            {
                if (identifiedGesture == spell.SpellGesture)
                {
                    spellReady = true;
                    selectedSpell = spell;
                    selectedSpell.OnAimStart();
                }
            }
            gesture.Clear();
            gesture3D.Clear();
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
