using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatLogic : MonoBehaviour
{
    public float frequency = 15f;

    public int initialForce = 15;
    public float initialAltitude;
    public float levAltitude = 24f;
    Vector3 m_EulerAngleVelocity;
    Rigidbody rb;
    public float timer = 6;

  // Use this for initialization
  void Start () {

        m_EulerAngleVelocity = new Vector3(-45, 100, 45);
        Rigidbody rb = GetComponent<Rigidbody>();
        initialAltitude = rb.worldCenterOfMass.y;    
        //this.enabled = false;
  }
  // Update is called once per frame
  void FixedUpdate () {
        Rigidbody rb = GetComponent<Rigidbody>();
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.deltaTime);

        timer -= Time.deltaTime;

        if (timer < 0)
        {
            timer = 5;
            this.enabled = false;
        }

        if (rb.worldCenterOfMass.y <initialAltitude+levAltitude){
            rb.AddForce(Vector3.up*initialForce, ForceMode.Impulse);
            rb.AddForce(0,frequency,0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        else
        {
            rb.velocity = new Vector3(0, 0, 0);

            rb.MoveRotation(rb.rotation * deltaRotation);
        }
  }
}
