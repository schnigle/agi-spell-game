using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogic : MonoBehaviour
{

    public GameObject muzzlePrefab, hitPrefab;
    public int force = 100;
    bool hit = false;
    private GameObject latesthitObject;


    void Start()
    {
       //hit = false;
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

    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();



    void OnCollisionEnter(Collision col)
    {

        ContactPoint contact = col.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;



        Debug.Log(col.gameObject.name);
        if(col.gameObject.tag== "Floatyobject"){
          latesthitObject = col.gameObject;
          latesthitObject.GetComponent<FloatLogic>().enabled = true;

        }

        if(hitPrefab != null)
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
                if(enemy != null)
                    enemy.isRagdolling = true;
            }
            rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                var direction = (rigidbody.transform.position - pos).normalized;
                rigidbody.AddForce((5 - Vector3.Distance(rigidbody.position, pos)) * (direction) * 200, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
