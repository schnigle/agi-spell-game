using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatLogic : MonoBehaviour
{

  public float frequency = 10f;
  public float frequency2 = 10f;


  // Position Storage Variables
  Vector3 posOffset = new Vector3 ();
  Vector3 tempPos = new Vector3 ();
  float timer = 5;
  // Use this for initialization
  void Start () {

    Rigidbody rb = GetComponent<Rigidbody>();
    rb.AddForce(0,100,0);
      // Store the starting position & rotation of the object
      //this.enabled = false;
    //  rb.AddForce(0,60,0);


  }

  // Update is called once per frame
  void FixedUpdate () {

    Rigidbody rb = GetComponent<Rigidbody>();

    timer -= Time.deltaTime;

    if(rb.worldCenterOfMass.y <5){
      rb.AddForce(0,frequency,0);
    }
    else{
      rb.AddForce(0,frequency2,0);
      if(timer<0){
        this.enabled = false;
      }
    }



      // Float up/down with a Sin()
    //  tempPos = posOffset;
    //  tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;

      //transform.position = tempPos;
  }
}
