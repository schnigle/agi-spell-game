using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatLogic : MonoBehaviour
{
    public float frequency = 15f;
    public int frequency2 = 9;
    public int initialForce = 15;
    public float initialAltitude;
    public float levAltitude = 4f;
  // Position Storage Variables
  Vector3 posOffset = new Vector3 ();
  Vector3 tempPos = new Vector3 ();
  public float timer = 3;
  // Use this for initialization
  void Start () {


    Rigidbody rb = GetComponent<Rigidbody>();
    initialAltitude = rb.worldCenterOfMass.y;
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

        if (rb.worldCenterOfMass.y <initialAltitude+levAltitude){
            rb.AddForce(Vector3.up*initialForce, ForceMode.Impulse);
            rb.AddForce(0,frequency,0);
        }
        else{
            rb.AddForce(Vector3.up*frequency2, ForceMode.Impulse);
        }

      // Float up/down with a Sin()
    //  tempPos = posOffset;
    //  tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;

      //transform.position = tempPos;
  }
}
