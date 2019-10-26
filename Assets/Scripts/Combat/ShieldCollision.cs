using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    Vector3 startSize;

    void Awake()
    {
        startSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, startSize, Time.deltaTime * 3f);
    }

    void OnCollisionEnter(Collision other)
    {
        Impact();
    }

    void OnTriggerEnter(Collider other)
    {
        Impact();
    }

    void Impact()
    {
        transform.localScale *= 1.1f;
    }
}
