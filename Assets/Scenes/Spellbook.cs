using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


namespace Valve.VR.InteractionSystem.Sample
{
    public class Spellbook : MonoBehaviour
    {
        public SteamVR_Action_Boolean m_move = null;
        public SteamVR_Action_Vector2 moveValue = null;



        public Hand hand;
        public GameObject prefabToPlant;

        private void Update()
        {
            touchpad();
        }

        private void touchpad()
        {
            // figure out orientation
            Vector3 orientationEuler = new Vector3(0, transform.eulerAngles.y, 0);
            Quaternion orientation = Quaternion.Euler(orientationEuler);
            Vector3 movement = Vector3.zero;


            // if not moving

            // if button pressed

            //Apply
        }

    }
}