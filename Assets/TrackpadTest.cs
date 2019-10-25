using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackpadTest : MonoBehaviour
{
    // [SerializeField]
    // SteamVR_Action_Vector2 alternativeTrackpadPosition;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // add Vector2 binding called "Trackpad" and assign it via bindings UI
        Vector2 trackpadPos = SteamVR_Input.GetVector2("Trackpad", SteamVR_Input_Sources.LeftHand);
        print(trackpadPos);
        // if (alternativeTrackpadPosition != null)
        // {
            // print(alternativeTrackpadPosition.axis);
        // }
        
    }
}
