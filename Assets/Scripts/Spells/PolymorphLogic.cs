using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolymorphLogic : MonoBehaviour
{
    public GameObject muzzlePrefab, hitPrefab, critter, critter2;
    private GameObject latesthitObject;

    void Start()
    {
        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
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
        Vector3 pos = contact.point;
        
        latesthitObject = col.gameObject;
        if (hitPrefab != null)
        {
            var hitVFX = Instantiate(hitPrefab, pos, rot);
            var psHit = hitVFX.GetComponent<ParticleSystem>();
            if (psHit != null)
            {
                Destroy(hitVFX, psHit.main.duration);
            }
            else
            {
                var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitVFX, psChild.main.duration);
            }
        }

        if (latesthitObject.GetComponent<Rigidbody>() != null)
        {
            float rng = Random.value;
            Debug.Log(rng);
            
            if (rng > 0.45 && rng < 0.55)
            {

                latesthitObject.transform.localScale += new Vector3(5, 5, 5);
            }

            else if (rng < 0.45)
            {
                Destroy(latesthitObject);
                Instantiate(critter, pos, Quaternion.identity);
            }
            else if(rng > 0.55)
            {
                Destroy(latesthitObject);
                Instantiate(critter2, pos, Quaternion.identity);
            }
            
            
            Debug.Log(pos);

        }
        Destroy(gameObject);
    }
}
