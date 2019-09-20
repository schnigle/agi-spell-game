using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogic2 : MonoBehaviour
{

    public GameObject muzzlePrefab, hitPrefab;
    private GameObject latesthitObject;

    void Start()
    {
        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            if(psMuzzle != null)
            {
                Destroy(muzzleVFX, psMuzzle.main.duration);
            }
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }

        }

        var colliderToIgnore = GameObject.Find("PlayerObject").GetComponent<Collider>(); // traversing hierarchy like a tree
        Physics.IgnoreCollision(colliderToIgnore, GetComponent<Collider>());
    }


    void Update()
    {

    }

    void OnCollisionEnter(Collision col)
    {

        ContactPoint contact = col.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;




        latesthitObject = col.gameObject;


        if (hitPrefab != null)
        {
            var hitVFX = Instantiate(hitPrefab, pos, rot);
            var psHit = hitVFX.GetComponent<ParticleSystem>();
            if(psHit != null)
            {
                Destroy(hitVFX, psHit.main.duration);
            }
            else
            {
                var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitVFX, psChild.main.duration);
            }
        }

       var nearby = Physics.OverlapSphere(pos, 5);
        foreach(var item in nearby)
        {
            Rigidbody rigidbody = null;
            if(item.tag == "Actor")
            {
                //print("Death good");
                var enemy = item.GetComponent<EnemyAI>();
                enemy.isRagdolling = true;
                item.GetComponent<FloatLogic>().enabled = true;

            }
            rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                latesthitObject.GetComponent<FloatLogic>().enabled = true;
            }
        }

        Destroy(gameObject);
    }
}
