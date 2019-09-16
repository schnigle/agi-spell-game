using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatLogic : MonoBehaviour
{
    public float frequency = 15f;
    public float frequency2 = 6.2f;
    public int initialForce = 24;
    public float levAltitude = 5f;

  // Position Storage Variables
  Vector3 posOffset = new Vector3 ();
  Vector3 tempPos = new Vector3 ();
  public float timer = 3;
  // Use this for initialization
  void Start () {


    Rigidbody rb = GetComponent<Rigidbody>();
    levAltitude = rb.worldCenterOfMass.y;
        //rb.AddForce(Vector3.up*1000, ForceMode.Impulse);
      // Store the starting position & rotation of the object
      //this.enabled = false;
    //  rb.AddForce(0,60,0);
  }

  // Update is called once per frame
  void FixedUpdate () {

    Rigidbody rb = GetComponent<Rigidbody>();

        timer -= Time.deltaTime;

        if (timer < 0)
        {
            timer = 5;
            this.enabled = false;
        }

        if (rb.worldCenterOfMass.y <levAltitude+6){
            rb.AddForce(Vector3.up*initialForce, ForceMode.Impulse);
            rb.AddForce(0,frequency,0);
        }
        else{
            rb.AddForce(0,frequency2,0);
        }

      // Float up/down with a Sin()
    //  tempPos = posOffset;
    //  tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;

      //transform.position = tempPos;
  }
}
