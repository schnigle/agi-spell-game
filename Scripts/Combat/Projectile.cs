using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    float lifeTime = 100;
    float lifeTimeRemaining;
    public GameObject caster;
    public Rigidbody RBody { get; private set; }

    void Awake()
    {
        RBody = GetComponent<Rigidbody>();
    }

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
        if (other.gameObject != caster)
        {
            // TODO: damage player, knock back enemies, VFX etc
            if (other.tag == "Actor")
            {
                print("projectile hit enemy");
            }
            else if (other.tag == "Player")
            {
                if (CameraPostProcessing.instance != null)
                {
                    CameraPostProcessing.instance.TriggerHit();
                }
                print("projectile hit player");
            }
            else
            {
                print("projectile hit ground");
            }
            Destroy(gameObject);
        }

    }
}
