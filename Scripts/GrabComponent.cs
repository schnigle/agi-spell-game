using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GrabComponent : MonoBehaviour
{
    private SteamVR_TrackedObject trackedObj;
    private Valve.VR.InteractionSystem.Hand hand;

    // Start is called before the first frame update
    void Start()
    {
        hand = gameObject.GetComponent<Valve.VR.InteractionSystem.Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
