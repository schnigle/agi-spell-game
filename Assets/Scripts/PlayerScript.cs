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

    bool aimingSpell = false;
    bool drawingGesture = false;
    GestureRecognition.Gesture identifiedGesture;
    ISpell selectedSpell = null;

    [SerializeField]
    Color inactiveStaffColor = Color.black;
    [SerializeField]
    Color activeStaffColor = Color.cyan;
    [SerializeField]
    StaffOrb staffOrb;
    [SerializeField]
    new GameObject collider;

    Transform gestureReferenceTransform;

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

        if(playerData == null)
            playerData = new PlayerData();

        trajectory.SetActive(false);
        trail.SetActive(false);
        if (staffOrb)
        {
            staffOrb.mainColor = inactiveStaffColor;
        }

        try
        {
            if(SteamVR.instance.hmd_ModelNumber == "Vive MV")
                resolution = new Tuple<int, int>(2160 / 2, 1200);
            else
                resolution = new Tuple<int, int>(2880 / 2, 1600);
        }
        catch (System.Exception)
        {
            Debug.Log("No VR headset has been identified. Defaulting to main screen dimensions.");
            resolution = new Tuple<int, int>(Screen.width, Screen.height);
        }
        controller.detectCollisions = false;
        gestureReferenceTransform = new GameObject().transform;
        gestureReferenceTransform.name = "Gesture Reference Transform";
    }

    private Tuple<int, int> resolution;

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
        bool trigger_down = SteamVR_Actions._default.Squeeze.GetAxis(rightInput) == 1 || Input.GetMouseButton(0);
        if (trigger_down)
        {
            // Trigger press start
            if (!trigger_down_last)
            {
                if (!aimingSpell)
                {
                    drawingGesture = true;
                    trail.SetActive(true);
                    trail.GetComponent<TrailRenderer>().Clear();
                    if (staffOrb)
                    {
                        staffOrb.StartDraw();
                        staffOrb.mainColor = activeStaffColor;
                    }
                    gestureReferenceTransform.transform.position = VRcamera.transform.position;
                    gestureReferenceTransform.transform.rotation = VRcamera.transform.rotation;
                }
                if(aimingSpell && selectedSpell != null)
                {
                    selectedSpell.OnAimEnd();
                    selectedSpell.UnleashSpell();
                    selectedSpell = null;
                    aimingSpell = false;
                    if (staffOrb)
                    {
                        staffOrb.mainColor = inactiveStaffColor;
                        staffOrb.iconEnabled = false;
                    }
                }
            }
            if (drawingGesture)
            {
                // Record gesture
                var pos = trail.transform.position;
                // Constant used to scale the transformed position closer to screen space scale. May not be very necessary. 
                const float transformScaleFactor = 2;
                // Transform the world space position to be relative to the reference transform.
                // The reference transform is a copy of the position and orientation of the 
                // camera in the beginning of the gesture.
                var transformedPos = gestureReferenceTransform.InverseTransformPoint(pos) * transformScaleFactor;
                // print("transformed position: " + transformedPos);

                var point = new GestureRecognition.Point_2D();
                var point_3D = new GestureRecognition.Point_3D();
                point.time = Time.time;
                point_3D.time = point.time;

                // var r = transformedPos.magnitude;
                // var sphericalCoords = new Vector3
                // (
                //     Mathf.Acos(transformedPos.z / r) * (transformedPos.x > 0 ? 1 : -1),
                //     Mathf.Atan2(transformedPos.y, transformedPos.z),
                //     r
                // );
                // print("spherical coords: " + sphericalCoords);
                // transformedPos = sphericalCoords;

                point.x = transformedPos.x;
                point.y = transformedPos.y;
                point.z = transformedPos.z;


                point_3D.x = pos.x;
                point_3D.y = pos.y;
                point_3D.z = pos.z;

                // print(point.x + ", " + point.y);
                gesture.Add(point);
                gesture3D.Add(point_3D);
            }

        }
        // Trigger press end
        else if (trigger_down_last)
        {
            if (drawingGesture)
            {
                drawingGesture = false;
                trail.SetActive(false);
                staffOrb.EndDraw();
                if (gesture.Count > 0 && gesture3D.Count > 0)
                {
                    GestureRecognition.Gesture_Meta result = gestureRecognition.recognize_gesture(gesture, gesture3D);
                    identifiedGesture = result.type;
                    print("Identified gesture: " + identifiedGesture);
                    foreach (var spell in GetComponents<ISpell>())
                    {
                        // A bit of an ugly check
                        MonoBehaviour spellComp = (MonoBehaviour)spell;
                        if (spellComp.enabled)
                        {
                            if (identifiedGesture == spell.SpellGesture)
                            {
                                aimingSpell = true;
                                selectedSpell = spell;
                                selectedSpell.OnAimStart();
                                staffOrb.mainColor = selectedSpell.OrbColor;
                                staffOrb.iconEnabled = true;
                            }
                        }
                    }
                }
                gesture.Clear();
                gesture3D.Clear();
                if (!aimingSpell)
                {
                    if (staffOrb)
                    {
                        staffOrb.mainColor = inactiveStaffColor;
                    }
                }
            }
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

        collider.transform.position = VRcamera.transform.position - Vector3.up*0.8f;
    }
}
