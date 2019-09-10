using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    float lifeTime = 100;
    float lifeTimeRemaining;
    // Start is called before the first frame update
    void Start()
    {
        lifeTimeRemaining = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimeRemaining -= Time.deltaTime;
        if (lifeTimeRemaining <= 0)
        {
            Destroy(gameObject);
        }
    }

    // void OnCollisionEnter(Collision other)
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Actor")
        {
            print("hit");
            // Destroy(other.gameObject);
        }

        Destroy(gameObject);
    }
}
