using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogic : MonoBehaviour
{

    public GameObject muzzlePrefab, hitPrefab;
    public int force = 100;
    private GameObject latesthitObject;

    public AudioClip explosion_clip;
    public AudioSource hit_pos_source;

    public LayerMask shieldMask;

    void Start()
    {
        hit_pos_source.clip = explosion_clip;
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


    void OnCollisionEnter(Collision col)
    {

        ContactPoint contact = col.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = GetComponent<Collider>().bounds.center;

        print("kalas");
        AudioSource.PlayClipAtPoint(explosion_clip, pos);

        Debug.Log(col.gameObject.name);


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
                if (!Physics.Raycast(transform.position, Vector3.Normalize(item.bounds.center - transform.position), Vector3.Distance(transform.position, item.bounds.center), shieldMask))
                {
                    var enemy = item.GetComponent<EnemyAI>();
                    enemy.isRagdolling = true;
                    enemy.Health -= 40;
                }
            }
            rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                var direction = (item.bounds.center - pos).normalized;
                rigidbody.AddForce((5 - Vector3.Distance(item.bounds.center, pos)) * (direction) * 200, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
