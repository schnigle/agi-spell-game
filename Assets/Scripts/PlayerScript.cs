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

    // related to visual feedback when spell succeeded
    private Color originalTrailColor;
    private float originalTrailTime;
    private Color originalTrailMaterialColor;
    private float trailAlphaDelta = 0.03f;
    private float fadeCounter = 0;
    private float originalTrailWidthMultiplier;

    // audio
    public AudioClip ambient_sounds;
    public AudioClip spell_successful_sound;
    public AudioClip spell_unsuccessful_sound;
    public AudioSource self_audio_source;

    Vector3 moveVector;
    CharacterController controller;

    public SteamVR_Input_Sources rightInput = SteamVR_Input_Sources.RightHand;

    PlayerData playerData;

    bool aimingSpell = false;
    bool drawingGesture = false;
    GestureRecognition.Gesture identifiedGesture;
    SpellBase selectedSpell = null;

    [SerializeField]
    Color inactiveStaffColor = Color.black;
    [SerializeField]
    Color activeStaffColor = Color.cyan;
    [SerializeField]
    StaffOrb staffOrb;
    [SerializeField]
    new GameObject collider;
    [SerializeField]
    Spellbook spellbook;
    [SerializeField]
    GameObject spellFailEffect;

    enum CoordinateSpace { screen, cameraStartTransform, sphericalCoordinates }
    [SerializeField]
    CoordinateSpace gestureCoordinateSpace = CoordinateSpace.cameraStartTransform;
    [SerializeField]
    bool ignoreGestureDepth;

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
        // trail
        originalTrailColor = trail.GetComponent<TrailRenderer>().startColor;
        originalTrailTime = trail.GetComponent<TrailRenderer>().time;
        originalTrailMaterialColor = trail.GetComponent<TrailRenderer>().material.color;
        originalTrailWidthMultiplier = trail.GetComponent<TrailRenderer>().widthMultiplier;
        print(originalTrailColor);

        //audio
        self_audio_source = GetComponent<AudioSource>();
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

    Vector3 WorldToScreenSpace(Vector3 worldPosition)
    {
            var pixelPos = VRcamera.WorldToScreenPoint(worldPosition);
            return new Vector3(
                pixelPos.x / resolution.Item1,
                pixelPos.y / resolution.Item2,
                pixelPos.z
            );
    }

    Vector3 WorldToReferenceSpace(Vector3 worldPosition)
    {
        // Constant used to scale the transformed position closer to screen space scale. May not be very necessary.
        const float transformScaleFactor = 2;
        // Transform the world space position to be relative to the reference transform.
        // The reference transform is a copy of the position and orientation of the
        // camera in the beginning of the gesture.
        var transformedPosition = gestureReferenceTransform.InverseTransformPoint(worldPosition) * transformScaleFactor;
        // Currently having trouble with depth using these coordinates
        return transformedPosition;
    }

    Vector3 WorldToSphericalCoordinates(Vector3 worldPosition)
    {
        const float transformScaleFactor = 1.5f;
        var transformedPosition = gestureReferenceTransform.InverseTransformPoint(worldPosition);
        /*
            x: side
            y: up
            z: forward
         */
        var r = transformedPosition.magnitude;
        var sphericalCoords = new Vector3
        (
            Mathf.Atan2(transformedPosition.x, transformedPosition.z) * transformScaleFactor,
            -(Mathf.Acos(transformedPosition.y / r) - Mathf.PI / 2) * transformScaleFactor,
            r
        );
        return sphericalCoords;
    }

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

                    // reset values from trail fade
                    trail.GetComponent<TrailRenderer>().startColor = originalTrailColor;
                    trail.GetComponent<TrailRenderer>().endColor = originalTrailColor;
                    trail.GetComponent<TrailRenderer>().material.color = originalTrailMaterialColor;
                    trail.GetComponent<TrailRenderer>().widthMultiplier = originalTrailWidthMultiplier;
                    trail.GetComponent<TrailRenderer>().emitting = true;
                    trail.GetComponent<TrailRenderer>().time = originalTrailTime;

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
                var orbPosition = trail.transform.position;
                Vector3 transformedPosition;
                if (gestureCoordinateSpace == CoordinateSpace.cameraStartTransform)
                {
                    transformedPosition = WorldToReferenceSpace(orbPosition);
                }
                else if (gestureCoordinateSpace == CoordinateSpace.sphericalCoordinates)
                {
                    transformedPosition = WorldToSphericalCoordinates(orbPosition);
                }
                else
                {
                    transformedPosition = WorldToScreenSpace(orbPosition);
                }
                if (ignoreGestureDepth)
                {
                    transformedPosition.z = 0;
                }
                // print("transformed position: " + transformedPosition);

                var point = new GestureRecognition.Point_2D();
                var point_3D = new GestureRecognition.Point_3D();
                point.time = Time.time;
                point_3D.time = point.time;

                point.x = transformedPosition.x;
                point.y = transformedPosition.y;
                point.z = transformedPosition.z;


                point_3D.x = orbPosition.x;
                point_3D.y = orbPosition.y;
                point_3D.z = orbPosition.z;

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
                staffOrb.EndDraw();
                if (gesture.Count > 0 && gesture3D.Count > 0)
                {
                    GestureRecognition.Gesture_Meta result = gestureRecognition.recognize_gesture(gesture, gesture3D);
                    identifiedGesture = result.type;
                    print("Identified gesture: " + identifiedGesture);
                    foreach (var spell in GetComponents<SpellBase>())
                    {
                        if (spell.enabled)
                        {
                            if (identifiedGesture == spell.SpellGesture)
                            {
                                aimingSpell = true;
                                selectedSpell = spell;
                                selectedSpell.OnAimStart();
                                staffOrb.mainColor = selectedSpell.OrbColor;
                                if (spell.SpellIcon != null)
                                {
                                    staffOrb.iconEnabled = true;
                                    staffOrb.SetIcon(spell.SpellIcon);
                                }

                                // Matched spell - play sound
                                self_audio_source.PlayOneShot(spell_successful_sound, 1.0f);

                                // Add to spell book
                                if (spellbook != null && spell.PageTexture != null && !spellbook.spellPageTextures.Contains(spell.PageTexture))
                                {
                                    spellbook.AddPage(spell.PageTexture, trail.GetComponent<TrailRenderer>().startColor);
                                }
                            }
                        }
                    }
                    if (selectedSpell == null && spellFailEffect != null)
                    {
                        var trailRend = trail.GetComponent<TrailRenderer>();
                        for (int i = 0; i < trailRend.positionCount; i++)
                        {
                            var startEffect = Instantiate(spellFailEffect);
                            startEffect.transform.position = trailRend.GetPosition(i);
                        }
                        self_audio_source.PlayOneShot(spell_unsuccessful_sound, 0.5f);

                    }

                    // If no spell matched just terminate the trail. Else "fade" the trail. 
                    if (!aimingSpell) {
                        trail.SetActive(false);
                    } else {
                        trail.GetComponent<TrailRenderer>().emitting = false;
                        fadeCounter = 0;
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

        // fade trail
        if (trail.activeSelf && !drawingGesture) {
            fadeCounter+=Time.deltaTime * 60;
            float newAlpha = Mathf.Sin(fadeCounter/21.2f) + 1.0f;
            Color newcolor = trail.GetComponent<TrailRenderer>().startColor;
            Color newmatcolor = trail.GetComponent<TrailRenderer>().material.color;
            newcolor.a = newAlpha;
            newmatcolor.a = newAlpha;
            trail.GetComponent<TrailRenderer>().startColor = newcolor;
            trail.GetComponent<TrailRenderer>().material.color = newmatcolor;
            trail.GetComponent<TrailRenderer>().widthMultiplier = originalTrailWidthMultiplier * newAlpha * 2.0f;

            if(fadeCounter > 100)
                trail.SetActive(false);
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
