﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWaveSpell : MonoBehaviour
{
    [SerializeField]
    GestureRecognition.Gesture gesture;
    public GestureRecognition.Gesture SpellGesture => gesture;
    [SerializeField]
    //Color color = Color.white;
    //public Color OrbColor => color;

    public GameObject bullet, bulletEmitter;
    public Transform playerTrans;
    public float forwardForce = 30.0f;
    private const float waitTime = 5.0f;

    [SerializeField]
    TrajectoryPreview trajectory;

    public void UnleashSpell()
    {
        GameObject tempBull;
        tempBull = Instantiate(bullet, bulletEmitter.transform.forward.normalized * 0.7f + bulletEmitter.transform.position, playerTrans.rotation) as GameObject;
        Rigidbody tempBody;
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
        if (Input.GetKeyDown("k"))
        {
            UnleashSpell();
        }


    }
}
