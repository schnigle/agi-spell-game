﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTornado : MonoBehaviour, ISpell
{
    [SerializeField]
    GestureRecognition.Gesture gesture;
	public GestureRecognition.Gesture SpellGesture => gesture;


    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float forwardForce = 30.0f;
    private const float waitTime = 15.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

    public void UnleashSpell()
    {
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.5f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
        tempBody = tempBull.GetComponent<Rigidbody>();
        tempBody.AddForce(bulletEmitter.transform.forward * forwardForce);
        Destroy(tempBull, waitTime);
    }

    public void OnAimStart()
    {
        trajectory?.gameObject.SetActive(true);
    }

    public void OnAimEnd()
    {
        trajectory?.gameObject.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            UnleashSpell();
        }


    }
}